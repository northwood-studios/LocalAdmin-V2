﻿using LocalAdmin.V2.Commands;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
        private const string VersionString = "2.2.2";
        private string? LocalAdminExecutable { get; set; }

        private readonly CommandService commandService = new CommandService();
        private Process? gameProcess;
        private TcpServer? server;
        private Task? readerTask;
        private string scpslExecutable = string.Empty;
        private ushort gamePort;
        private bool exit;

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

                    ReadInput((input) =>
                    {
                        if (!string.IsNullOrEmpty(input)) return ushort.TryParse(input, out gamePort);
                        gamePort = 7777;
                        return true;

                    }, () => { }, () =>
                    {
                        ConsoleUtil.WriteLine("Port number must be a unsigned short integer.", ConsoleColor.Red);
                    });
                }
                else
                {
                    if (!ushort.TryParse(args[0], out gamePort))
                    {
                        ConsoleUtil.WriteLine("Failed - Invalid port!");

                        Exit();
                    }
                }

                Console.Title = "LocalAdmin v. " + VersionString + " on port " + gamePort;

                SetupPlatform();
                RegisterCommands();
                SetupReader();

                Menu();

                ConsoleUtil.WriteLine("Started new session.", ConsoleColor.DarkGreen);
                ConsoleUtil.WriteLine("Trying to start server...", ConsoleColor.Gray);

                SetupServer();
                
                while (server!.ConsolePort == 0)
                    Thread.Sleep(200);
                
                RunScpsl();

                readerTask!.Start();

                Task.WaitAll(readerTask);

                Exit(0); // After the readerTask is completed this will happen
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
            ConsoleUtil.WriteLine("Copyright by KernelError and zabszk, 2019 - 2020", ConsoleColor.Cyan);
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
            server = new TcpServer();
            server.Received += (sender, line) =>
            {
                if (!byte.TryParse(line.AsSpan(0, 1), NumberStyles.HexNumber, null, out var colorValue))
                    colorValue = (byte) ConsoleColor.Gray;

                ConsoleUtil.WriteLine(line.Substring(1, line.Length - 1), (ConsoleColor)colorValue);
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

        private void RunScpsl()
        {
            if (File.Exists(scpslExecutable))
            {
                ConsoleUtil.WriteLine("Executing: " + scpslExecutable, ConsoleColor.DarkGreen);

                var startInfo = new ProcessStartInfo
                {
                    FileName = scpslExecutable,
                    Arguments = $"-batchmode -nographics -nodedicateddelete -port{gamePort} -console{server!.ConsolePort} -id{Process.GetCurrentProcess().Id}",
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
            commandService.RegisterCommand(new NewCommand(LocalAdminExecutable!));
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

        private void Exit(int code = -1)
        {
            server!.Stop();
            gameProcess!.Kill(); // Forcefully terminating the process
            Environment.Exit(code);
        }

        /*private void Restart()
        {
            server!.Stop();
            Process.Start(LocalAdminExecutable, gamePort.ToString());
            Environment.Exit(0);
        }*/
    }
}