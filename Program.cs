using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        private const string VersionString = "1.0";

        public static string localAdminExecutable;

        public static void Main(string[] args)
        {
			ConsoleUtil.WriteLock = new object();
			Console.Title = "LocalAdmin 2 - " + VersionString;

			try
	        {
		        var portSelected = false;
		        ushort port = 0;

				if (args.Length == 0)
		        {
			        ConsoleUtil.Write("You can pass port number as first startup argument.", ConsoleColor.Green);
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
			        Console.Title = "LocalAdmin 2 - " + VersionString + " on port " + port;
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
				        while (true)
				        {
					        var input = Console.ReadLine();
					        var currentLineCursor = Console.CursorTop;
					        Console.SetCursorPosition(0, Console.CursorTop - 1);
					        Console.Write(new string(' ', Console.WindowWidth));
					        ConsoleUtil.Write(">>> " + input, ConsoleColor.DarkMagenta, -1);
					        Console.SetCursorPosition(0, currentLineCursor);

					        var split = input?.ToUpper().Split(' ');

					        if (split.Length > 0)
					        {
						        var name = split[0];
						        var arguments = split.Skip(1).ToArray();

						        var command = commandService.GetCommandByName(name);

						        if (command != null)
							        command.Execute(arguments);
						        else
							        client.Write(input);
					        }
				        }
			        });

			        var memoryWatcherTask = new Task(() =>
			        {
				        while (true)
				        {
					        if (gameProcess != null && !gameProcess.HasExited)
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

							        Process.Start(localAdminExecutable);
							        gameProcess?.Kill();
						        }
					        }

					        Task.Delay(2000);
				        }
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
			        ConsoleUtil.Write("SCP: Secret Laboratory - LocalAdmin 2 - " + VersionString,
				        ConsoleColor.Cyan);
			        ConsoleUtil.Write("");
			        ConsoleUtil.Write(
				        "Licensed under The MIT License (use command \"license\" to get license text).",
				        ConsoleColor.DarkGray);
			        ConsoleUtil.Write("Copyright by KernelError and zabszk, 2019",
				        ConsoleColor.DarkGray);
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
				        var color = 8;
				        if (eventArgs.Contains("LOGTYPE"))
				        {
					        var num = eventArgs.Remove(0, eventArgs.IndexOf("LOGTYPE", StringComparison.Ordinal) + 7);
					        color = int.Parse(num.Contains("-") ? num.Remove(0, 1) : num);

					        eventArgs = eventArgs.Remove(eventArgs.IndexOf("LOGTYPE", StringComparison.Ordinal) + 9);
				        }

				        ConsoleUtil.Write(
					        eventArgs.Contains("LOGTYPE")
						        ? eventArgs.Substring(0, eventArgs.Length - 9)
						        : eventArgs, (ConsoleColor) color);
			        };

			        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => { gameProcess?.Kill(); };

			        if (gameProcess != null)
				        gameProcess.Exited += (obj, eventArgs) =>
				        {
					        var startInfo = new ProcessStartInfo(localAdminExecutable, args[0]);
					        Process.Start(startInfo);
					        Environment.Exit(0);
				        };

			        //Run coroutines
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