using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public const string VersionString = "2.1.1";
        public string LocalAdminExecutable { get; private set; }

        private CommandService commandService = new CommandService();

        private Process gameProcess;
        private COMClient client;

        private Task memoryWatcherTask;
        private Task readerTask;

        private string scpslExecutable = "";
        private string session = "";

        private int tooLowMemory = 0;
        private ushort port = 0;

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
                        return ushort.TryParse(input, out port);
                    }, () => { }, () =>
                    {
                        ConsoleUtil.WriteLine("Port number must be a unsigned short integer.", ConsoleColor.Red);
                    });
                } else
                {
                    if(!ushort.TryParse(args[0], out port))
                    {
                        ConsoleUtil.WriteLine("Failed - Invalid port!");

                        Exit();
                    }
                }

                Console.Title = "LocalAdmin v. " + VersionString + " on port " + port;

                SetupPlatform();
                RegisterCommands();
                SetupMemoryWatcher();
                SetupReader();

                session = Randomizer.RandomString(20);

                SetupFiles();
                Menu();

                ConsoleUtil.WriteLine("Started new session.", ConsoleColor.DarkGreen);
                ConsoleUtil.WriteLine("Trying to start server...", ConsoleColor.Gray);

                RunSCPSL(port);
                SetupClient();

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

        private void SetupFiles()
        {
            if (Directory.Exists("SCPSL_DATA/Dedicated/" + session))
            {
                try
                {
                    Directory.Delete("SCPSL_DATA/Dedicated/" + session, true);
                }
                catch (IOException)
                {
                    ConsoleUtil.WriteLine("Failed - Please close all open files in SCPSL_Data/Dedicated and restart the server!", ConsoleColor.Red);
                    ConsoleUtil.WriteLine("Press any key to close...", ConsoleColor.DarkGray);

                    Exit();
                }
            }

            Directory.CreateDirectory("SCPSL_Data/Dedicated/" + session);
        }

        private void SetupClient()
        {
            client = new COMClient(session);
            client.Received += (sender, eventArgs) =>
            {
                var color = 7;

                if (eventArgs.Contains("LOGTYPE"))
                {
                    var num = eventArgs.Remove(0, eventArgs.LastIndexOf("LOGTYPE", StringComparison.Ordinal) + 7);

                    color = int.Parse(num.Contains("-") ? num.Remove(0, 1) : num);
                    eventArgs = eventArgs.Remove(eventArgs.LastIndexOf("LOGTYPE", StringComparison.Ordinal) + 9);
                }

                ConsoleUtil.WriteLine(eventArgs.Contains("LOGTYPE") ? eventArgs.Substring(0, eventArgs.Length - 9) : eventArgs, (ConsoleColor)color);
            };
        }

        private void SetupReader()
        {
            readerTask = new Task(async () =>
            {
                while (client == null)
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
                        client.WriteLine(input);
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
                    Arguments = $"-batchmode -nographics -key{session} -nodedicateddelete -port{port} -id{Process.GetCurrentProcess().Id}",
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
            client.Dispose();

            Console.ReadKey(true);
            Environment.Exit(code);
        }

        private void Restart()
        {
            client.Dispose();

            Process.Start(LocalAdminExecutable, port.ToString());
            Environment.Exit(0);
        }
    }
}