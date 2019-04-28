using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LocalAdmin_V2_Net_Core.Commands;

namespace LocalAdmin_V2_Net_Core
{
    internal class Program
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

        private const string VersionString = "2.1.1";

        public static string localAdminExecutable;
        private static bool _exit;

        public static void Main(string[] args)
        {
			ConsoleUtil.Init();
			Console.Title = "LocalAdmin v. " + VersionString;

			try
	        {
		        var portSelected = false;
		        ushort port = 0;

				if (args.Length == 0)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("You can pass port number as first startup argument.");
			        Console.WriteLine("");

					while (!portSelected)
					{
						Console.ForegroundColor = ConsoleColor.Green;
				        Console.Write("Port number (default: 7777): ");

				        var read = Console.ReadLine();
				        if (read == "")
				        {
					        port = 7777;
					        portSelected = true;
					        break;
				        }
				        
						portSelected = ushort.TryParse(read, out port);
						if (!portSelected)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Port number must be a unsigned short integer.");
						}
					}
		        }

		        if (portSelected || ushort.TryParse(args[0], out port))
		        {
			        Console.Title = "LocalAdmin v. " + VersionString + " on port " + port;
			        var scpslExecutable = "";

			        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			        {
				        scpslExecutable = "SCPSL.exe";
				        localAdminExecutable = "LocalAdmin.exe";
			        }
			        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			        {
				        scpslExecutable = "SCPSL.x86_64";
				        localAdminExecutable = "LocalAdmin.x86_x64";
			        }
			        else
			        {
				        ConsoleUtil.Write("Failed - Unsupported platform!", ConsoleColor.Red);

				        Console.ReadKey(true);

				        Environment.Exit(-1);
			        }

					Process gameProcess = null;
			        var commandService = new CommandService();

			        var session = "";

			        var tooLowMemory = 0;

			        commandService.RegisterCommand(new NewCommand());
			        commandService.RegisterCommand(new HelpCommand());
			        commandService.RegisterCommand(new LicenseCommand());

			        COMClient client = null;

			        var readerTask = new Task(() =>
			        {
						while(client == null) Thread.Sleep(20);
				        while (!_exit)
				        {
					        var input = Console.ReadLine();
					        if (string.IsNullOrWhiteSpace(input)) continue;
					        var currentLineCursor = Console.CursorTop;
					        Console.SetCursorPosition(0, Console.CursorTop - 1);
					        Console.Write(new string(' ', Console.WindowWidth));
					        ConsoleUtil.Write(">>> " + input, ConsoleColor.DarkMagenta, -1);
					        Console.SetCursorPosition(0, currentLineCursor);

					        var split = input.ToUpper().Split(' ');

					        if (split.Length <= 0) continue;
					        var name = split[0];
					        var arguments = split.Skip(1).ToArray();

					        var command = commandService.GetCommandByName(name);

					        if (input.ToLower() == "exit")
						        _exit = true;

					        if (command != null)
						        command.Execute(arguments);
					        else
						        client.Write(input);
				        }
			        });

			        var memoryWatcherTask = new Task(() =>
			        {
				        while (!_exit)
				        {
					        Thread.Sleep(2000);

							if (gameProcess == null) continue;
							if (!gameProcess.HasExited)
					        {
						        gameProcess.Refresh();
						        var UsedRAM_MB = (int) (gameProcess.WorkingSet64 / 1048576);

						        if (UsedRAM_MB < 400 && gameProcess.StartTime.AddMinutes(3) < DateTime.Now)
							        tooLowMemory++;
						        else
							        tooLowMemory = 0;

						        if (tooLowMemory > 5 || gameProcess.MainWindowTitle != "")
						        {
							        ConsoleUtil.Write("Session crashed. Trying to restart dedicated server...",
								        ConsoleColor.Red);

							        gameProcess?.Kill();
									Process.Start(localAdminExecutable, port.ToString());	
									Environment.Exit(0);
						        }

						        continue;
					        }

							ConsoleUtil.Write("Session crashed. Trying to restart dedicated server...",
								ConsoleColor.Red);

							Process.Start(localAdminExecutable, port.ToString());
							Environment.Exit(0);
						}

						if (gameProcess != null)
							while (!gameProcess.HasExited)
								Thread.Sleep(100);

						ConsoleUtil.Write("Game process successfully exited.", ConsoleColor.DarkGreen);
						ConsoleUtil.Write("Exiting LocalAdmin...", ConsoleColor.DarkGreen);

						ConsoleUtil.Terminate();
			        });

			        session = Randomizer.RandomString(20);

			        #region "Create a place for new files"

			        if (Directory.Exists("SCPSL_DATA/Dedicated/" + session))
				        try
				        {
					        Directory.Delete("SCPSL_DATA/Dedicated/" + session, true);
				        }
				        catch (IOException)
				        {
					        ConsoleUtil.Write(
						        "Failed - Please close all open files in SCPSL_Data/Dedicated and restart the server!",
						        ConsoleColor.Red);
					        ConsoleUtil.Write("Press any key to close...", ConsoleColor.DarkGray);

					        Console.ReadKey(true);

					        Environment.Exit(-1);
				        }

			        #endregion

			        #region "Menu"

			        Console.Clear();
			        ConsoleUtil.Write("SCP: Secret Laboratory - LocalAdmin v. " + VersionString,
				        ConsoleColor.Cyan);
			        ConsoleUtil.Write("");
			        ConsoleUtil.Write(
				        "Licensed under The MIT License (use command \"license\" to get license text).",
				        ConsoleColor.Cyan);
			        ConsoleUtil.Write("Copyright by KernelError and zabszk, 2019",
				        ConsoleColor.Cyan);
			        ConsoleUtil.Write("");
			        ConsoleUtil.Write("Type 'help' to get list of available commands.", ConsoleColor.Cyan);
			        ConsoleUtil.Write("");

			        #endregion

			        Directory.CreateDirectory("SCPSL_Data/Dedicated/" + session);

			        ConsoleUtil.Write("Started new session.", ConsoleColor.DarkGreen);
			        ConsoleUtil.Write("Trying to start server...", ConsoleColor.Gray);

			        if (File.Exists(scpslExecutable))
			        {
				        //Run SCPSL
				        ConsoleUtil.Write("Executing: " + scpslExecutable, ConsoleColor.DarkGreen);
				        gameProcess = Process.Start(scpslExecutable,
					        "-batchmode -nographics -key" + session + " -nodedicateddelete -port" + port + " -id" +
					        Process.GetCurrentProcess().Id);
			        }
			        else
			        {
				        ConsoleUtil.Write("Failed - Executable file not found!", ConsoleColor.Red);
				        ConsoleUtil.Write("Press any key to close...", ConsoleColor.DarkGray);

				        Console.ReadKey(true);

				        Environment.Exit(-1);
			        }

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

						ConsoleUtil.Write(
							eventArgs.Contains("LOGTYPE")
								? eventArgs.Substring(0, eventArgs.Length - 9)
								: eventArgs, (ConsoleColor)color);
					};


			        //Run
			        readerTask.Start();
			        memoryWatcherTask.Start();

			        Task.WaitAll(readerTask, memoryWatcherTask);
				}
		        else
		        {
			        ConsoleUtil.Write("Failed - Invalid port!");

			        Console.ReadKey(true);
		        }
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
    }
}