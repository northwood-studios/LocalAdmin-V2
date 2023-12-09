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
            MigrateArgsFile();

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

        private static void MigrateArgsFile()
        {
            try
            {
                const string OldFileLocation = "laargs.txt";

                if (!File.Exists(StartupArgsPath))
                {
                    if (!File.Exists(OldFileLocation))
                        return;
                    else
                        File.Move(OldFileLocation, StartupArgsPath);
                }
                else
                    File.Delete(OldFileLocation);
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"An error occured while trying to migrate 'laargs.txt': {ex}");
            }
        }
    }
}
