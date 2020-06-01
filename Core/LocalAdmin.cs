using LocalAdmin.V2.Commands;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.IO.NativeSignalHandlers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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

    public sealed class LocalAdmin
    {
        public const string _VersionString = "2.2.2";
        public static readonly LocalAdmin Singleton = new LocalAdmin();

        public string? LocalAdminExecutable { get; private set; }
        public ushort GamePort { get; private set; }

        private readonly CommandService commandService = new CommandService();
        private Process? gameProcess;
        private TcpServer? server;
        private Task? readerTask;
        private string? scpslExecutable;
        private bool exit;

        public void Start(string[] args)
        {
            Console.Title = $"LocalAdmin v. {_VersionString}";

            try
            {
                ushort port = 0;
                if (args.Length == 0)
                {
                    ConsoleUtil.WriteLine("You can pass port number as first startup argument.", ConsoleColor.Green);
                    Console.WriteLine(string.Empty);
                    ConsoleUtil.Write("Port number (default: 7777): ", ConsoleColor.Green);

                    ReadInput((input) =>
                    {
                        if (!string.IsNullOrEmpty(input))
                            return ushort.TryParse(input, out port);
                        port = 7777;
                        return true;

                    }, () => { }, () =>
                    {
                        ConsoleUtil.WriteLine("Port number must be a unsigned short integer.", ConsoleColor.Red);
                    });
                }
                else
                {
                    if (!ushort.TryParse(args[0], out port))
                    {
                        ConsoleUtil.WriteLine("Failed - Invalid port!");

                        Exit();
                    }
                }

                Console.Title = $"LocalAdmin v. {_VersionString} on port {port}";

                SetupPlatform();
                RegisterCommands();
                SetupReader();

                StartSession(port);

                readerTask!.Start();

                Task.WaitAll(readerTask);

                Exit(0); // After the readerTask is completed this will happen
            }
            catch (Exception ex)
            {
                File.WriteAllText($"{DateTimeOffset.UtcNow:yyyy-MM-DDTHH-mm-ssZ}-crash.txt", ex.ToString());

                /*
                Logger.Log("|===| Exception |===|");
                Logger.Log("Time: " + DateTime.Now);
                Logger.Log(ex);
                Logger.Log("|===================|");
                Logger.Log("");
                */
            }
        }

        /// <summary>
        ///     Starts a session,
        ///     if the session has already begun,
        ///     then terminates it.
        /// </summary>
        public void StartSession(ushort port)
        {
            // Terminate the game, if the game process is exists
            if (gameProcess != null && !gameProcess.HasExited)
                TerminateGame();

            Menu();

            ConsoleUtil.WriteLine("Started new session.", ConsoleColor.DarkGreen);
            ConsoleUtil.WriteLine("Trying to start server...", ConsoleColor.Gray);

            SetupServer();

            while (server!.ConsolePort == 0)
                Thread.Sleep(200);

            GamePort = port;
            RunScpsl(port);
        }

        private void Menu()
        {
            ConsoleUtil.Clear();
            ConsoleUtil.WriteLine($"SCP: Secret Laboratory - LocalAdmin v. {_VersionString}", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("Licensed under The MIT License (use command \"license\" to get license text).", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine("Copyright by KernelError and zabszk, 2019 - 2020", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("Type 'help' to get list of available commands.", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine(string.Empty);
        }

        private void SetupPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                scpslExecutable = "SCPSL.exe";
                LocalAdminExecutable = "LocalAdmin.exe";
                WindowsNTHandler.Handler.Setup();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                scpslExecutable = "SCPSL.x86_64";
                LocalAdminExecutable = "LocalAdmin.x86_x64";
                UnixHandler.Handler.Setup();
            }
            else
            {
                ConsoleUtil.WriteLine("Failed - Unsupported platform!", ConsoleColor.Red);

                Exit(10); // According to MSDN, it's the ERROR_BAD_ENVIRONMENT error code
            }
        }

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

                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ConsoleUtil.Write(new string(' ', Console.WindowWidth));
                    ConsoleUtil.WriteLine(">>> " + input, ConsoleColor.DarkMagenta, -1);
                    Console.SetCursorPosition(0, currentLineCursor);

                    var split = input.Split(' ');

                    if (split.Length == 0) continue;
                    var name = split[0].ToUpper(CultureInfo.InvariantCulture);
                    var arguments = split.Skip(1).ToArray();

                    var command = commandService.GetCommandByName(name);

                    if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                        exit = true;

                    if (command != null)
                        command.Execute(arguments);
                    else if (server.Connected)
                        server.WriteLine(input);
                    else ConsoleUtil.WriteLine("Failed to send command - connection to server process hasn't been established yet.", ConsoleColor.Yellow);
                }
            });
        }

        private void RunScpsl(ushort port)
        {
            if (File.Exists(scpslExecutable))
            {
                ConsoleUtil.WriteLine("Executing: " + scpslExecutable, ConsoleColor.DarkGreen);

                var startInfo = new ProcessStartInfo
                {
                    FileName = scpslExecutable,
                    Arguments = $"-batchmode -nographics -nodedicateddelete -port{port} -console{server!.ConsolePort} -id{Process.GetCurrentProcess().Id}",
                    CreateNoWindow = true
                };

                gameProcess = Process.Start(startInfo);
            }
            else
            {
                ConsoleUtil.WriteLine("Failed - Executable file not found!", ConsoleColor.Red);
                ConsoleUtil.WriteLine("Press any key to close...", ConsoleColor.DarkGray);

                Exit(2); // According to MSDN, it's the ERROR_FILE_NOT_FOUND error code
            }
        }

        private void RegisterCommands()
        {
            commandService.RegisterCommand(new RestartCommand());
            commandService.RegisterCommand(new NewCommand());
            commandService.RegisterCommand(new HelpCommand());
            commandService.RegisterCommand(new LicenseCommand());
        }

        private static void ReadInput(Func<string, bool> checkInput, Action validInputAction, Action invalidInputAction)
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
            server!.Stop();
            gameProcess!.Kill();
        }

        /// <summary>
        ///     Terminates the game and console.
        /// </summary>
        public void Exit(int code = -1)
        {
            TerminateGame(); // Forcefully terminating the process
            Environment.Exit(code);
        }
    }
}