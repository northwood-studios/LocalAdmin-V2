using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LocalAdmin.V2.Commands;

namespace LocalAdmin.V2
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

    internal class LocalAdmin
    {
        public const string VersionString = "2.2.0";
        public string LocalAdminExecutable { get; private set; }

        private CommandService commandService = new CommandService();

        private Process gameProcess;

        private TcpServer server;

        private Task memoryWatcherTask;
        private Task readerTask;

        private string scpslExecutable = "";

        private int tooLowMemory;
        private ushort gamePort;
        private ushort consolePort;

        private bool _exit;

        public void Start(string[] args)
        {
            Console.Title = "LocalAdmin v. " + VersionString;

            try
            {
                if (args.Length == 0)
                {
                    ConsoleUtil.WriteLine("You can pass port number as first startup argument.", ConsoleColor.Green);
                    Console.WriteLine("");
                    ConsoleUtil.Write("Port number (default: 7777): ", ConsoleColor.Green);

                    var userInput = ReadInput((input) =>
                    {
                        if(input == "")
                        {
                            gamePort = 7777;

                            return true;
                        }
                            
                        return ushort.TryParse(input, out gamePort);
                    }, () => { }, () =>
                    {
                        ConsoleUtil.WriteLine("Port number must be a unsigned short integer.", ConsoleColor.Red);
                    });
                } else
                {
                    if(!ushort.TryParse(args[0], out gamePort))
                    {
                        ConsoleUtil.WriteLine("Failed - Invalid port!");

                        Exit();
                    }
                }

                Console.Title = "LocalAdmin v. " + VersionString + " on port " + gamePort;

                SetupPlatform();
                RegisterCommands();
                SetupMemoryWatcher();
                SetupReader();

                Menu();

                ConsoleUtil.WriteLine("Started new session.", ConsoleColor.DarkGreen);
                ConsoleUtil.WriteLine("Trying to start server...", ConsoleColor.Gray);

                consolePort = GetFirstFreePort();

                RunSCPSL(gamePort);
                SetupServer();

                readerTask.Start();
                memoryWatcherTask.Start();

                Task.WaitAll(readerTask, memoryWatcherTask);
            }
            catch (Exception ex)
            {
                Logger.Log("|===| Exception |===|");
                Logger.Log("Time: " + DateTime.Now);
                Logger.Log(ex);
                Logger.Log("|===================|");
                Logger.Log("");
            }
        }

        private void Menu()
        {
            ConsoleUtil.Clear();
            ConsoleUtil.WriteLine("SCP: Secret Laboratory - LocalAdmin v. " + VersionString, ConsoleColor.Cyan);
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("Licensed under The MIT License (use command \"license\" to get license text).", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine("Copyright by KernelError and zabszk, 2019", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("Type 'help' to get list of available commands.", ConsoleColor.Cyan);
            ConsoleUtil.WriteLine("");
        }

        private void SetupPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                scpslExecutable = "SCPSL.exe";
                LocalAdminExecutable = "LocalAdmin.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                scpslExecutable = "SCPSL.x86_64";
                LocalAdminExecutable = "LocalAdmin.x86_x64";
            }
            else
            {
                ConsoleUtil.WriteLine("Failed - Unsupported platform!", ConsoleColor.Red);

                Exit();
            }
        }
        
        private void SetupServer()
        {
            server = new TcpServer(gamePort);
            server.Received += (sender, line) =>
            {
                var colorValue = byte.Parse(Convert.ToString(line[0]), NumberStyles.HexNumber);
                var color = (ConsoleColor)colorValue;

                ConsoleUtil.WriteLine(line.Substring(1, line.Length - 2), color);
            };
            server.Start();
        }

        private void SetupReader()
        {
            readerTask = new Task(async () =>
            {
                while (server == null)
                    await Task.Delay(20);

                while (!_exit)
                {
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    var currentLineCursor = Console.CursorTop;

                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ConsoleUtil.Write(new string(' ', Console.WindowWidth));
                    ConsoleUtil.WriteLine(">>> " + input, ConsoleColor.DarkMagenta, -1);
                    Console.SetCursorPosition(0, currentLineCursor);

                    var split = input.ToUpper().Split(' ');

                    if (split.Length <= 0)
                        continue;

                    var name = split[0];
                    var arguments = split.Skip(1).ToArray();

                    var command = commandService.GetCommandByName(name);

                    if (input.ToLower() == "exit")
                        _exit = true;

                    if (command != null)
                        command.Execute(arguments);
                    else
                        server.WriteLine(input);
                }
            });
        }

        private void SetupMemoryWatcher()
        {
            memoryWatcherTask = new Task(async () =>
            {
                while (!_exit)
                {
                    await Task.Delay(2000);

                    if (gameProcess == null) continue;
                    if (!gameProcess.HasExited)
                    {
                        gameProcess.Refresh();
                        var UsedRAM_MB = (int)(gameProcess.WorkingSet64 / 1048576);

                        if (UsedRAM_MB < 400 && gameProcess.StartTime.AddMinutes(3) < DateTime.Now)
                            tooLowMemory++;
                        else
                            tooLowMemory = 0;

                        if (tooLowMemory > 5 || gameProcess.MainWindowTitle != "")
                        {
                            ConsoleUtil.WriteLine("Session crashed. Trying to restart dedicated server...", ConsoleColor.Red);

                            gameProcess?.Kill();

                            Restart();
                        }

                        continue;
                    }

                    ConsoleUtil.WriteLine("Session crashed. Trying to restart dedicated server...", ConsoleColor.Red);

                    Restart();
                }

                if (gameProcess != null)
                    while (!gameProcess.HasExited)
                        Thread.Sleep(100);

                ConsoleUtil.WriteLine("Game process successfully exited.", ConsoleColor.DarkGreen);
                ConsoleUtil.WriteLine("Exiting LocalAdmin...", ConsoleColor.DarkGreen);
            });
        }

        private void RunSCPSL(ushort port)
        {
            if (File.Exists(scpslExecutable))
            {
                ConsoleUtil.WriteLine("Executing: " + scpslExecutable, ConsoleColor.DarkGreen);

                var startInfo = new ProcessStartInfo
                {
                    FileName = scpslExecutable,
                    Arguments = $"-batchmode -nographics -nodedicateddelete -port{gamePort} -console{consolePort} -id{Process.GetCurrentProcess().Id}",
                    CreateNoWindow = true
                };

                gameProcess = Process.Start(startInfo);
            }
            else
            {
                ConsoleUtil.WriteLine("Failed - Executable file not found!", ConsoleColor.Red);
                ConsoleUtil.WriteLine("Press any key to close...", ConsoleColor.DarkGray);

                Exit();
            }
        }

        private void RegisterCommands()
        {
            commandService.RegisterCommand(new NewCommand(LocalAdminExecutable));
            commandService.RegisterCommand(new HelpCommand());
            commandService.RegisterCommand(new LicenseCommand());
        }

        private string ReadInput(Func<string, bool> checkInput, Action validInputAction, Action invalidInputAction)
        {
            var input = Console.ReadLine();

            while (!checkInput(input))
            {
                invalidInputAction();

                input = Console.ReadLine();
            }

            validInputAction();

            return input;
        }

        private void Exit(int code = -1)
        {
            server.Stop();

            Console.ReadKey(true);
            Environment.Exit(code);
        }

        private void Restart()
        {
            server.Stop();

            Process.Start(LocalAdminExecutable, gamePort.ToString());
            Environment.Exit(0);
        }
        
        private ushort GetFirstFreePort()
        {
            var activeTcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
        
            for (ushort port = 10000; port < ushort.MaxValue; port++)
            {
                if (activeTcpListeners.All(listener => listener.Port != port))
                {
                    return port;
                }
            }

            throw new Exception("No free port!");
        }
    }
}