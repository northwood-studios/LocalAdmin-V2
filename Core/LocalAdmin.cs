using LocalAdmin.V2.Commands;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.IO.ExitHandlers;
using System;
using System.Collections.Concurrent;
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
        public const string VersionString = "2.4.2";
        public static LocalAdmin? Singleton;
        public static ushort GamePort;
        public static string? ConfigPath, LaLogsPath, GameLogsPath;
        private static bool _firstRun = true;

        private readonly CommandService _commandService = new CommandService();
        private Process? _gameProcess;
        internal TcpServer? Server { get; private set; }
        private Task? _readerTask;
        private readonly string _scpslExecutable;
        private static string _gameArguments = string.Empty;
        internal static string BaseWindowTitle = $"LocalAdmin v. {VersionString}";
        internal static readonly string GameUserDataRoot =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar +
            "SCP Secret Laboratory" + Path.DirectorySeparatorChar;
        private static bool _exit, _processRefreshFail;
        private static readonly ConcurrentQueue<string> InputQueue = new ConcurrentQueue<string>();
        internal static bool NoSetCursor, PrintControlMessages, AutoFlush = true, EnableLogging = true, NoPadding = false;
        private static bool _stdPrint;
        private volatile bool _processClosing;

        private static int _restarts = -1, _restartsLimit = 4, _restartsTimeWindow = 480; //480 seconds = 8 minutes
        private static bool _ignoreNextRestart;
        private static readonly Stopwatch RestartsStopwatch = new Stopwatch();

        internal static ulong LogLengthLimit = 25000000000, LogEntriesLimit = 10000000000; 

        internal static Config? Configuration;

        internal ShutdownAction ExitAction = ShutdownAction.Crash;

        internal enum ShutdownAction : byte
        {
            Crash,
            Shutdown,
            SilentShutdown,
            Restart
        }

        private enum CaptureArgs : byte
        {
            None,
            ArgsPassthrough,
            ConfigPath,
            LaLogsPath,
            GameLogsPath,
            RestartsLimit,
            RestartsTimeWindow,
            LogLengthLimit,
            LogEntriesLimit
        }

        internal LocalAdmin()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _scpslExecutable = "SCPSL.exe";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                _scpslExecutable = "SCPSL.x86_64";
            else
            {
                ConsoleUtil.WriteLine("Failed - Unsupported platform!", ConsoleColor.Red);
                // shut up dotnet
                _scpslExecutable = string.Empty;
                Exit(1);
            }
        }

        public void Start(string[] args)
        {
            Singleton = this;
            Console.Title = BaseWindowTitle;

            if (_restartsLimit > -1)
            {
                if (_restartsTimeWindow > 0)
                {
                    if (!RestartsStopwatch.IsRunning)
                        RestartsStopwatch.Start();
                    else if (RestartsStopwatch.Elapsed.TotalSeconds > _restartsTimeWindow)
                    {
                        RestartsStopwatch.Restart();
                        _restarts = 0;
                    }
                }

                if (!_ignoreNextRestart)
                    _restarts++;
                else _ignoreNextRestart = false;

                if (_restarts > _restartsLimit)
                {
                    ConsoleUtil.WriteLine("Restarts limit exceeded.", ConsoleColor.Red);
                    ExitAction = ShutdownAction.SilentShutdown;
                    Exit(1);
                }
            }

            try
            {
                var reconfigure = false;
                var useDefault = false;
                
                if (_firstRun)
                {
                    if (args.Length == 0 || !ushort.TryParse(args[0], out GamePort))
                    {
                        ConsoleUtil.WriteLine("You can pass port number as first startup argument.",
                            ConsoleColor.Green);
                        Console.WriteLine(string.Empty);
                        ConsoleUtil.Write("Port number (default: 7777): ", ConsoleColor.Green);

                        ReadInput((input) =>
                            {
                                if (!string.IsNullOrEmpty(input))
                                    return ushort.TryParse(input, out GamePort);
                                GamePort = 7777;
                                return true;

                            }, () => { },
                            () =>
                            {
                                ConsoleUtil.WriteLine("Port number must be a unsigned short integer.",
                                    ConsoleColor.Red);
                            });
                    }

                    var capture = CaptureArgs.None;

                    foreach (var arg in args)
                    {
                        switch (capture)
                        {
                            case CaptureArgs.None:
                                if (arg.StartsWith("-", StringComparison.Ordinal) &&
                                    !arg.StartsWith("--", StringComparison.Ordinal) && arg.Length > 1)
                                {
                                    for (var i = 1; i < arg.Length; i++)
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

                                            case 's':
                                                _stdPrint = true;
                                                break;

                                            case 'd':
                                                useDefault = true;
                                                break;
                                            
                                            case 'a':
                                                NoPadding = true;
                                                break;
                                        }
                                    }
                                }
                                else
                                    switch (arg)
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

                                        case "--printStd":
                                            _stdPrint = true;
                                            break;

                                        case "--useDefault":
                                            useDefault = true;
                                            break;
                                        
                                        case "--noAlign":
                                            NoPadding = true;
                                            break;

                                        case "--config":
                                            capture = CaptureArgs.ConfigPath;
                                            break;
                                        
                                        case "--logs":
                                            capture = CaptureArgs.LaLogsPath;
                                            break;
                                        
                                        case "--gameLogs":
                                            capture = CaptureArgs.GameLogsPath;
                                            break;
                                        
                                        case "--restartsLimit":
                                            capture = CaptureArgs.RestartsLimit;
                                            break;
                                        
                                        case "--restartsTimeWindow":
                                            capture = CaptureArgs.RestartsTimeWindow;
                                            break;
                                        
                                        case "--logLengthLimit":
                                            capture = CaptureArgs.LogLengthLimit;
                                            break;
                                        
                                        case "--logEntriesLimit":
                                            capture = CaptureArgs.LogEntriesLimit;
                                            break;

                                        case "--":
                                            capture = CaptureArgs.ArgsPassthrough;
                                            break;
                                    }
                                break;

                            case CaptureArgs.ArgsPassthrough:
                                _gameArguments += $"\"{arg}\" ";
                                break;

                            case CaptureArgs.ConfigPath:
                                ConfigPath = arg;
                                capture = CaptureArgs.None;
                                break;

                            case CaptureArgs.LaLogsPath:
                                LaLogsPath = arg + Path.DirectorySeparatorChar;
                                capture = CaptureArgs.None;
                                break;

                            case CaptureArgs.GameLogsPath:
                                GameLogsPath = arg + Path.DirectorySeparatorChar;
                                capture = CaptureArgs.None;
                                break;
                            
                            case CaptureArgs.RestartsLimit:
                                if (!int.TryParse(arg, out _restartsLimit) || _restartsLimit < -1)
                                {
                                    _restartsLimit = 4;
                                    ConsoleUtil.WriteLine("restartsLimit argument value must be an integer greater or equal to -1.", ConsoleColor.Red);
                                }
                                capture = CaptureArgs.None;
                                break;
                            
                            case CaptureArgs.RestartsTimeWindow:
                                if (!int.TryParse(arg, out _restartsTimeWindow) || _restartsLimit < 0)
                                {
                                    _restartsTimeWindow = 480;
                                    ConsoleUtil.WriteLine("restartsTimeWindow argument value must be an integer greater or equal to 0.", ConsoleColor.Red);
                                }
                                capture = CaptureArgs.None;
                                break;

                            case CaptureArgs.LogLengthLimit:
                            {
                                string a = arg.Replace("k", "000", StringComparison.Ordinal)
                                    .Replace("M", "000000", StringComparison.Ordinal)
                                    .Replace("G", "000000000", StringComparison.Ordinal)
                                    .Replace("T", "000000000000", StringComparison.Ordinal);
                                if (!ulong.TryParse(a, out LogLengthLimit))
                                {
                                    ConsoleUtil.WriteLine(
                                        "logLengthLimit argument value must be an integer greater or equal to 0.",
                                        ConsoleColor.Red);
                                    LogLengthLimit = 25000000000;
                                }

                                capture = CaptureArgs.None;
                            }
                                break;

                            case CaptureArgs.LogEntriesLimit:
                            {
                                string a = arg.Replace("k", "000", StringComparison.Ordinal)
                                    .Replace("M", "000000", StringComparison.Ordinal)
                                    .Replace("G", "000000000", StringComparison.Ordinal)
                                    .Replace("T", "000000000000", StringComparison.Ordinal);
                                if (!ulong.TryParse(a, out LogEntriesLimit))
                                {
                                    ConsoleUtil.WriteLine(
                                        "logEntriesLimit argument value must be an integer greater or equal to 0.",
                                        ConsoleColor.Red);
                                    LogEntriesLimit = 10000000000;
                                }

                                capture = CaptureArgs.None;
                            }
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                if (ConfigPath != null)
                {
                    if (File.Exists(ConfigPath))
                        Configuration = Config.DeserializeConfig(File.ReadAllLines(ConfigPath, Encoding.UTF8));
                    else reconfigure = true;
                }
                else
                {
                    var cfgPath =
                        $"{GameUserDataRoot}config{Path.DirectorySeparatorChar}{GamePort}{Path.DirectorySeparatorChar}config_localadmin.txt";

                    if (File.Exists(cfgPath))
                        Configuration = Config.DeserializeConfig(File.ReadAllLines(cfgPath, Encoding.UTF8));
                    else
                    {
                        cfgPath = $"{GameUserDataRoot}config{Path.DirectorySeparatorChar}config_localadmin_global.txt";

                        if (File.Exists(cfgPath))
                            Configuration = Config.DeserializeConfig(File.ReadAllLines(cfgPath, Encoding.UTF8));
                        else
                            reconfigure = true;
                    }
                }

                if (reconfigure)
                    ConfigWizard.RunConfigWizard(useDefault);

                NoSetCursor |= Configuration!.LaNoSetCursor;
                AutoFlush &= Configuration!.LaLogAutoFlush;
                EnableLogging &= Configuration!.EnableLaLogs;
                
                InputQueue.Clear();

                if (_firstRun)
                {
                    try
                    {
                        SetupExitHandlers();
                    }
                    catch (Exception ex)
                    {
                        ConsoleUtil.WriteLine(
                            $"Starting exit handlers threw {ex}. Game process will NOT be closed on console closing!",
                            ConsoleColor.Yellow);
                    }
                }

                if (_firstRun || _exit)
                {
                    _exit = false;
                    _firstRun = false;
                    SetupKeyboardInput();
                }
                
                RegisterCommands();
                SetupReader();

                StartSession();

                _readerTask!.Start();
                
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
                
                while (!_exit)
                    Thread.Sleep(250);

                // If the game was terminated intentionally, then wait, otherwise no
                Exit(0, true); // After the readerTask is completed this will happen
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
        private void StartSession()
        {
            // Terminate the game, if the game process is exists
            if (_gameProcess != null && !_gameProcess.HasExited)
                TerminateGame();

            Menu();

            BaseWindowTitle = $"LocalAdmin v. {VersionString} on port {GamePort}";
            Console.Title = BaseWindowTitle;
            
            Logger.Initialize();

            ConsoleUtil.WriteLine($"Started new session on port {GamePort}.", ConsoleColor.DarkGreen);
            ConsoleUtil.WriteLine("Trying to start server...", ConsoleColor.Gray);

            SetupServer();

            while (Server!.ConsolePort == 0)
                Thread.Sleep(200);
            
            RunScpsl();
        }

        private void Menu()
        {
            ConsoleUtil.Clear();
            ConsoleUtil.WriteLine($"SCP: Secret Laboratory - LocalAdmin v. {VersionString}", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("Licensed under The MIT License (use command \"license\" to get license text).", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine("Copyright by KernelError and Łukasz \"zabszk\" Jurczyk, 2019 - 2021", ConsoleColor.Cyan);
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
            Server = new TcpServer();
            Server.Received += (sender, line) =>
            {
                if (!byte.TryParse(line.AsSpan(0, 1), NumberStyles.HexNumber, null, out var colorValue))
                    colorValue = (byte)ConsoleColor.Gray;

                ConsoleUtil.WriteLine(line[1..], (ConsoleColor)colorValue);
            };
            Server.Start();
        }

        private void SetupKeyboardInput()
        {
            new Task(() =>
            {
                while (!_exit)
                {
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    InputQueue.Enqueue(input);
                }
            }).Start();
        }

        private void SetupReader()
        {
            _readerTask = new Task(async () =>
            {
                while (Server == null)
                    await Task.Delay(20);

                while (!_exit)
                {
                    if (!InputQueue.TryDequeue(out var input))
                    {
                        await Task.Delay(65);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(input))
                        continue;
                    
                    var currentLineCursor = NoSetCursor ? 0 : Console.CursorTop;

                    if (currentLineCursor > 0)
                    {
                        Console.SetCursorPosition(0, currentLineCursor - 1);
                        
                        ConsoleUtil.WriteLine($"{string.Empty.PadLeft(Console.WindowWidth)}>>> {input}", ConsoleColor.DarkMagenta, -1);
                        Console.SetCursorPosition(0, currentLineCursor);
                    }
                    else
                        ConsoleUtil.WriteLine($">>> {input}", ConsoleColor.DarkMagenta, -1);

                    if (!_processRefreshFail && _gameProcess != null)
                    {
                        try
                        {
                            if (_gameProcess.HasExited)
                            {
                                ConsoleUtil.WriteLine("Failed to send command - the game process was terminated...",
                                    ConsoleColor.Red);
                                _exit = true;
                                continue;
                            }
                        }
                        catch
                        {
                            _processRefreshFail = true;
                        }
                    }

                    var split = input.Split(' ');

                    if (split.Length == 0)
                        continue;
                    var name = split[0].ToUpperInvariant();

                    var command = _commandService.GetCommandByName(name);

                    if (command != null)
                    {
                        command.Execute(split.Skip(1).ToArray());
                        if (!command.SendToGame)
                            continue;
                    }

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                        input.StartsWith("exit ", StringComparison.OrdinalIgnoreCase) ||
                        input.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                        input.StartsWith("quit ", StringComparison.OrdinalIgnoreCase) ||
                        input.Equals("stop", StringComparison.OrdinalIgnoreCase) ||
                        input.StartsWith("stop ", StringComparison.OrdinalIgnoreCase))
                    {
                        ExitAction = ShutdownAction.SilentShutdown;
                        _exit = true;
                    }

                    if (Server.Connected)
                        Server.WriteLine(input);
                    else
                        ConsoleUtil.WriteLine(
                            "Failed to send command - connection to server process hasn't been established yet.",
                            ConsoleColor.Yellow);
                }
            });
        }

        private void RunScpsl()
        {
            if (File.Exists(_scpslExecutable))
            {
                ConsoleUtil.WriteLine("Executing: " + _scpslExecutable, ConsoleColor.DarkGreen);
                var printStd = Configuration!.LaShowStdoutStderr || _stdPrint;
                var redirectStreams =
                    Configuration!.LaShowStdoutStderr || printStd;
                

                var startInfo = new ProcessStartInfo
                {
                    FileName = _scpslExecutable,
                    Arguments =
                        $"-batchmode -nographics -nodedicateddelete -port{GamePort} -console{Server!.ConsolePort} -id{Environment.ProcessId} {_gameArguments}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _gameProcess = Process.Start(startInfo);

                _gameProcess!.OutputDataReceived += (sender, args) =>
                {
                    if (!redirectStreams || string.IsNullOrWhiteSpace(args.Data))
                        return;

                    ConsoleUtil.WriteLine("[STDOUT] " + args.Data, ConsoleColor.Gray,
                        log: Configuration!.LaLogStdoutStderr,
                        display: printStd);
                };

                _gameProcess!.ErrorDataReceived += (sender, args) =>
                {
                    if (!redirectStreams || string.IsNullOrWhiteSpace(args.Data))
                        return;

                    ConsoleUtil.WriteLine("[STDERR] " + args.Data, ConsoleColor.DarkMagenta,
                        log: Configuration!.LaLogStdoutStderr,
                        display: printStd);
                };

                _gameProcess!.BeginOutputReadLine();
                _gameProcess!.BeginErrorReadLine();

                _gameProcess!.Exited += (sender, args) =>
                {
                    try
                    {
                        if (_gameProcess != null && !_gameProcess.HasExited)
                        {
                            ConsoleUtil.WriteLine("Game process exited and has been killed.", ConsoleColor.Gray);
                            _gameProcess.Kill();
                        }
                        else ConsoleUtil.WriteLine("Game process exited and has been killed, no need to kill it.", ConsoleColor.Gray);
                    }
                    catch (Exception e)
                    {
                        ConsoleUtil.WriteLine($"Game process exited and has been killed, can't kill it: {e.Message}.", ConsoleColor.Gray);
                    }
                    
                    if (_processClosing)
                        return;

                    switch (ExitAction)
                    {
                        case ShutdownAction.Crash:
                            ConsoleUtil.WriteLine("The game process has been terminated...", ConsoleColor.Red);
                            Exit(0, true);
                            break;
                        
                        case ShutdownAction.Shutdown:
                            Exit(0, true);
                            break;
                        
                        case ShutdownAction.SilentShutdown:
                            Exit(0);
                            break;
                        
                        case ShutdownAction.Restart:
                            Exit(0, false, true);
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };

                _gameProcess!.EnableRaisingEvents = true;
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
            _commandService.RegisterCommand(new RestartCommand());
            _commandService.RegisterCommand(new ForceRestartCommand());
            _commandService.RegisterCommand(new HelpCommand());
            _commandService.RegisterCommand(new LicenseCommand());
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
            Server?.Stop();
            if (_gameProcess != null && !_gameProcess.HasExited)
                _gameProcess.Kill();
        }

        /// <summary>
        ///     Terminates the game and console.
        /// </summary>
        public void Exit(int code = -1, bool waitForKey = false, bool restart = false)
        {
            lock (this)
            {
                if (_processClosing)
                    return;

                _exit = true;
                _processClosing = true;
                LogCleaner.Abort();
                Logger.EndLogging();
                TerminateGame(); // Forcefully terminating the process
                _gameProcess?.Dispose();
                
                try
                {
                    if (_readerTask != null && _readerTask.IsCompleted)
                        _readerTask?.Dispose();
                }
                catch
                {
                    //Ignore
                }

                if (restart || ExitAction == ShutdownAction.Restart)
                {
                    _ignoreNextRestart = true;
                    return;
                }

                if (ExitAction == ShutdownAction.Crash && Configuration != null && Configuration.RestartOnCrash)
                    return;
                
                if (waitForKey && ExitAction != ShutdownAction.SilentShutdown)
                {
                    ConsoleUtil.WriteLine("Press any key to close...", ConsoleColor.DarkGray);
                    Console.ReadKey(true);
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
