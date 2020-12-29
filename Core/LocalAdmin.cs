using LocalAdmin.V2.Commands;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.IO.ExitHandlers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalAdmin.V2.IO.Logging;

namespace LocalAdmin.V2.Core
{
    /*
        * Console colors:
        * Gray - LocalAdmin log
        * Red - critical error
        * DarkGray - insignificant info
        * Cyan - Header or important tip
        * Yellow - warning
        * DarkGreen - success
        * Blue - normal SCPSL log
    */

    public sealed class LocalAdmin : IDisposable
    {
        public const string VersionString = "2.3.1";
        public static readonly LocalAdmin Singleton = new LocalAdmin();
        public ushort GamePort;

        private readonly CommandService commandService = new CommandService();
        private Process? gameProcess;
        private TcpServer? server;
        private Task? readerTask;
        private readonly string scpslExecutable;
        private string gameArguments = string.Empty;
        internal static string BaseWindowTitle = $"LocalAdmin v. {VersionString}";
        internal static readonly string GameUserDataRoot =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar +
            "SCP Secret Laboratory" + Path.DirectorySeparatorChar;
        private bool exit;
        internal static bool NoSetCursor, PrintControlMessages, AutoFlush = true, EnableLogging = true;
        private volatile bool processClosing;

        internal static Config? Configuration;

        private LocalAdmin()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                scpslExecutable = "SCPSL.exe";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                scpslExecutable = "SCPSL.x86_64";
            else
            {
                ConsoleUtil.WriteLine("Failed - Unsupported platform!", ConsoleColor.Red);
                // shut up dotnet
                scpslExecutable = "";
                Exit(1);
            }
        }

        public void Start(string[] args)
        {
            Console.Title = BaseWindowTitle;

            try
            {
                if (args.Length == 0 || !ushort.TryParse(args[0], out GamePort))
                {
                    ConsoleUtil.WriteLine("You can pass port number as first startup argument.", ConsoleColor.Green);
                    Console.WriteLine(string.Empty);
                    ConsoleUtil.Write("Port number (default: 7777): ", ConsoleColor.Green);

                    ReadInput((input) =>
                    {
                        if (!string.IsNullOrEmpty(input))
                            return ushort.TryParse(input, out GamePort);
                        GamePort = 7777;
                        return true;

                    }, () => { }, () =>
                    {
                        ConsoleUtil.WriteLine("Port number must be a unsigned short integer.", ConsoleColor.Red);
                    });
                }

                var passArgs = false;
                var reconfigure = false;
                
                foreach (var arg in args)
                {
                    if (passArgs)
                    {
                        gameArguments += $"\"{arg}\" ";
                        continue;
                    }

                    if (arg.StartsWith("-", StringComparison.Ordinal) &&
                        !arg.StartsWith("--", StringComparison.Ordinal) && arg.Length > 1)
                    {
                        for (int i = 1; i < arg.Length; i++)
                        {
                            switch (arg[i])
                            {
                                case 'c':
                                    NoSetCursor = true;
                                    break;
                        
                                case 'p':
                                    PrintControlMessages = true;
                                    break;
                        
                                case 'n':
                                    AutoFlush = false;
                                    break;
                        
                                case 'l':
                                    EnableLogging = false;
                                    break;
                        
                                case 'r':
                                    reconfigure = true;
                                    break;
                            }
                        }
                    }
                    else switch (arg)
                    {
                        case "--noSetCursor":
                            NoSetCursor = true;
                            break;
                        
                        case "--printControl":
                            PrintControlMessages = true;
                            break;
                        
                        case "--noAutoFlush":
                            AutoFlush = false;
                            break;
                        
                        case "--noLogs":
                            EnableLogging = false;
                            break;
                        
                        case "--reconfigure":
                            reconfigure = true;
                            break;
                        
                        case "--":
                            passArgs = true;
                            break;
                    }
                }

                var cfgPath = $"{GameUserDataRoot}config{Path.PathSeparator}{GamePort}{Path.PathSeparator}config_localadmin.txt";
                
                if (File.Exists(cfgPath))
                    Configuration = Config.DeserializeConfig(File.ReadAllLines(cfgPath, Encoding.UTF8));
                else
                {
                    cfgPath = $"{GameUserDataRoot}config{Path.PathSeparator}config_localadmin_global.txt";
                    
                    if (File.Exists(cfgPath))
                        Configuration = Config.DeserializeConfig(File.ReadAllLines(cfgPath, Encoding.UTF8));
                    else
                        reconfigure = true;
                }
                
                if (reconfigure)
                    ConfigWizard.RunConfigWizard();

                NoSetCursor |= Configuration!.LaNoSetCursor;
                AutoFlush &= Configuration!.LaLogAutoFlush;
                EnableLogging &= Configuration!.EnableLaLogs;

                try
                {
                    SetupExitHandlers();
                }
                catch (Exception ex)
                {
                    ConsoleUtil.WriteLine($"Starting exit handlers threw {ex}. Game process will NOT be closed on console closing!", ConsoleColor.Yellow);
                }

                RegisterCommands();
                SetupReader();

                StartSession();

                readerTask!.Start();
                
                if (!EnableLogging)
                    ConsoleUtil.WriteLine("Logging has been disabled.", ConsoleColor.Red);
                else if (!AutoFlush)
                    ConsoleUtil.WriteLine("Logs auto flush has been disabled.", ConsoleColor.Yellow);
                
                if (PrintControlMessages)
                    ConsoleUtil.WriteLine("Printing control messages been enabled using startup argument.", ConsoleColor.Gray);
                
                if (NoSetCursor)
                    ConsoleUtil.WriteLine("Cursor management been disabled.", ConsoleColor.Gray);
                
                if (Configuration.LaDeleteOldLogs || Configuration.DeleteOldRoundLogs || Configuration.CompressOldRoundLogs)
                    LogCleaner.Initialize();

                Task.WaitAll(readerTask);

                // If the game was terminated intentionally, then wait, otherwise no
                Exit(0, gameProcess != null && gameProcess.HasExited); // After the readerTask is completed this will happen
            }
            catch (Exception ex)
            {
                File.WriteAllText($"LocalAdmin Crash {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ssZ}.txt", ex.ToString());
                
                Logger.Log("|===| Exception |===|");
                Logger.Log(ex);
                Logger.Log("|===================|");
                Logger.Log("");
            }
        }

        /// <summary>
        /// Starts a session,
        /// if the session has already begun,
        /// then terminates it.
        /// </summary>
        public void StartSession(ushort port = 0)
        {
            // Terminate the game, if the game process is exists
            if (gameProcess != null && !gameProcess.HasExited)
                TerminateGame();

            Menu();

            if (port != 0)
                GamePort = port;

            BaseWindowTitle = $"LocalAdmin v. {VersionString} on port {GamePort}";
            Console.Title = BaseWindowTitle;
            
            Logger.Initialize();

            ConsoleUtil.WriteLine($"Started new session on port {GamePort}.", ConsoleColor.DarkGreen);
            ConsoleUtil.WriteLine("Trying to start server...", ConsoleColor.Gray);

            SetupServer();

            while (server!.ConsolePort == 0)
                Thread.Sleep(200);
            
            RunScpsl();
        }

        private void Menu()
        {
            ConsoleUtil.Clear();
            ConsoleUtil.WriteLine($"SCP: Secret Laboratory - LocalAdmin v. {VersionString}", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("Licensed under The MIT License (use command \"license\" to get license text).", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine("Copyright by KernelError and Łukasz \"zabszk\" Jurczyk, 2019 - 2020", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("Type 'help' to get list of available commands.", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine(string.Empty);
        }

        private static void SetupExitHandlers()
        {
            ProcessHandler.Handler.Setup();
            AppDomainHandler.Handler.Setup();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsHandler.Handler.Setup();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
#if LINUX_SIGNALS
                try
                {
                    UnixHandler.Handler.Setup();
                }
                catch (DllNotFoundException ex)
                {
                    if (!CheckMonoException(ex)) throw;
                }
                catch (EntryPointNotFoundException ex)
                {
                    if (!CheckMonoException(ex)) throw;
                }
                catch (TypeInitializationException ex)
                {
                    switch (ex.InnerException)
                    {
                        case DllNotFoundException dll:
                            if (!CheckMonoException(dll)) throw;
                            break;
                        case EntryPointNotFoundException dll:
                            if (!CheckMonoException(dll)) throw;
                            break;
                        default:
                            throw;
                    }
                }
#else
                ConsoleUtil.WriteLine("Invalid Linux build! Please download LocalAdmin from an official source!", ConsoleColor.Red);
#endif
            }
        }

#if LINUX_SIGNALS
        private static bool CheckMonoException(Exception ex)
        {
            if (!ex.Message.Contains("MonoPosixHelper")) return false;
            ConsoleUtil.WriteLine("Native exit handling for Linux requires Mono to be installed!", ConsoleColor.Yellow);
            return true;
        }
#endif

        private void SetupServer()
        {
            server = new TcpServer();
            server.Received += (sender, line) =>
            {
                if (!byte.TryParse(line.AsSpan(0, 1), NumberStyles.HexNumber, null, out var colorValue))
                    colorValue = (byte)ConsoleColor.Gray;

                ConsoleUtil.WriteLine(line[1..], (ConsoleColor)colorValue);
            };
            server.Start();
        }

        private void SetupReader()
        {
            readerTask = new Task(async () =>
            {
                while (server == null)
                    await Task.Delay(20);

                while (!exit)
                {
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    var currentLineCursor = Console.CursorTop;

                    if (!NoSetCursor && currentLineCursor > 0)
                    {
                        Console.SetCursorPosition(0, currentLineCursor - 1);
                        ConsoleUtil.Write(string.Empty.PadLeft(Console.WindowWidth));
                        ConsoleUtil.WriteLine($">>> {input}", ConsoleColor.DarkMagenta, -1);
                        Console.SetCursorPosition(0, currentLineCursor);
                    }
                    else
                        ConsoleUtil.WriteLine($">>> {input}", ConsoleColor.DarkMagenta, -1);

                    if (input.StartsWith("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        exit = true;
                        continue;
                    }

                    if (gameProcess != null && gameProcess.HasExited)
                    {
                        ConsoleUtil.WriteLine("Failed to send command - the game process was terminated...", ConsoleColor.Red);
                        exit = true;
                        continue;
                    }

                    var split = input.Split(' ');

                    if (split.Length == 0)
                        continue;
                    var name = split[0].ToUpperInvariant();

                    var command = commandService.GetCommandByName(name);

                    if (command != null)
                    {
                        command.Execute(split.Skip(1).ToArray());
                        if (!command.SendToGame)
                            continue;
                    }

                    if (server.Connected)
                        server.WriteLine(input);
                    else
                        ConsoleUtil.WriteLine("Failed to send command - connection to server process hasn't been established yet.", ConsoleColor.Yellow);
                }
            });
        }

        private void RunScpsl()
        {
            if (File.Exists(scpslExecutable))
            {
                ConsoleUtil.WriteLine("Executing: " + scpslExecutable, ConsoleColor.DarkGreen);
                var redirectStreams = Configuration!.LaShowStdoutStderr || Configuration!.LaLogStdoutStderr;
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = scpslExecutable,
                    Arguments = $"-batchmode -nographics -nodedicateddelete -port{GamePort} -console{server!.ConsolePort} -id{Process.GetCurrentProcess().Id} {gameArguments}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = redirectStreams,
                    RedirectStandardError = redirectStreams,
                };

                gameProcess = Process.Start(startInfo);

                if (!redirectStreams) return;
                gameProcess!.OutputDataReceived += (sender, args) =>
                {
                    if (string.IsNullOrWhiteSpace(args.Data))
                        return;

                    ConsoleUtil.WriteLine("[STDOUT] " + args.Data, ConsoleColor.Gray, log: Configuration!.LaLogStdoutStderr, display: Configuration!.LaShowStdoutStderr);
                };
                
                gameProcess!.ErrorDataReceived += (sender, args) =>
                {
                    if (string.IsNullOrWhiteSpace(args.Data))
                        return;

                    ConsoleUtil.WriteLine("[STDERR] " + args.Data, ConsoleColor.DarkMagenta, log: Configuration!.LaLogStdoutStderr, display: Configuration!.LaShowStdoutStderr);
                };
                
                gameProcess!.BeginOutputReadLine();
                gameProcess!.BeginErrorReadLine();

                gameProcess!.Exited += (sender, args) =>
                {
                    if (processClosing)
                        return;
                    
                    ConsoleUtil.WriteLine("The game process has been terminated...", ConsoleColor.Red);
                    Exit(0, true);
                };

                gameProcess!.EnableRaisingEvents = true;
            }
            else
            {
                ConsoleUtil.WriteLine("Failed - Executable file not found!", ConsoleColor.Red);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Exit((int)WindowsErrorCode.ERROR_FILE_NOT_FOUND, true);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    Exit((int)UnixErrorCode.ERROR_FILE_NOT_FOUND, true);
                else
                    Exit(1);
            }
        }

        private void RegisterCommands()
        {
            commandService.RegisterCommand(new RestartCommand());
            commandService.RegisterCommand(new NewCommand());
            commandService.RegisterCommand(new HelpCommand());
            commandService.RegisterCommand(new LicenseCommand());
        }

        private static void ReadInput(Func<string?, bool> checkInput, Action validInputAction, Action invalidInputAction)
        {
            var input = Console.ReadLine();

            while (!checkInput(input))
            {
                invalidInputAction();

                input = Console.ReadLine();
            }

            validInputAction();
        }

        /// <summary>
        ///     Terminates the game.
        /// </summary>
        private void TerminateGame()
        {
            server?.Stop();
            if (gameProcess != null && !gameProcess.HasExited)
                gameProcess.Kill();
        }

        /// <summary>
        ///     Terminates the game and console.
        /// </summary>
        public void Exit(int code = -1, bool waitForKey = false)
        {
            lock (this)
            {
                if (processClosing)
                    return;

                processClosing = true;
                LogCleaner.Abort();
                Logger.EndLogging();
                TerminateGame(); // Forcefully terminating the process
                gameProcess?.Dispose();
                readerTask?.Dispose();
                if (waitForKey)
                {
                    ConsoleUtil.WriteLine("Press any key to close...", ConsoleColor.DarkGray);
                    Console.Read();
                }
                Environment.Exit(code);
            }
        }

        /// <summary>
        ///     Releases resources used by the program
        /// </summary>
        public void Dispose()
        {
            Exit(0);
            GC.SuppressFinalize(this);
        }

        ~LocalAdmin()
        {
            Exit(0);
        }
    }
}
