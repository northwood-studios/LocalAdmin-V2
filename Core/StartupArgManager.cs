using LocalAdmin.V2.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalAdmin.V2.Core
{
    public static class StartupArgManager
    {
        /// <summary>
        /// Path to startup arguments file.
        /// </summary>
        private static readonly string StartupArgsPath = Path.Combine(PathManager.ConfigPath, "laargs.txt");
        /// <summary>
        /// Merges Command-line arguments and arguments in <paramref name="cmdArgs"/>
        /// </summary>
        /// <param name="cmdArgs">Runtime Command-Line Arguments</param>
        /// <returns>Merged Arguments</returns>
        public static string[] MergeStartupArgs(IEnumerable<string> cmdArgs)
        {
            List<string> startupArgs = new List<string>();
            startupArgs.AddRange(cmdArgs);

            try
            {
                if (!File.Exists(StartupArgsPath))
                    return startupArgs.ToArray();

                startupArgs.AddRange(File.ReadAllLines(StartupArgsPath).Where(arg => !arg.StartsWith("#", StringComparison.Ordinal)));
                return startupArgs.ToArray();
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"An error occured while trying to merge arguments: {ex}", ConsoleColor.Red);
                return startupArgs.ToArray();
            }
        }

        public static void MigrateArgsFile()
        {
            try
            {
                string OldFileLocation = "laargs.txt";

                if (!File.Exists(StartupArgsPath))
                {
                    Console.WriteLine("File No Exist!");
                    return;
                }
                if (string.IsNullOrEmpty(File.ReadAllText(OldFileLocation)) || string.IsNullOrWhiteSpace(File.ReadAllText(OldFileLocation)))
                {
                    Console.WriteLine("File in old location but empty.");
                    File.Delete(OldFileLocation);
                }
                Console.WriteLine("File in old location.");

                // Deletes the laargs.txt in the new location which basically screws the Move operation.
                File.Delete(StartupArgsPath);

                File.WriteAllText(StartupArgsPath, File.ReadAllText(OldFileLocation));
                File.Delete(OldFileLocation);
                
                return;
            }
            catch (Exception ex)
            {
                // I have a massive skill issue and can't make this work without needing a try block. I am soley to blame.
                ConsoleUtil.WriteLine($"An error occured while trying to migrate laargs.txt: {ex}", ConsoleColor.Red);
                Console.ReadKey();
            }
        }
    }
}
