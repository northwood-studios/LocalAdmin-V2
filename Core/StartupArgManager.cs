using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalAdmin.V2.IO;

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

            string[] startupArgs = [.. cmdArgs];

            try
            {
                if (!File.Exists(StartupArgsPath))
                    return [.. startupArgs];

                return [.. startupArgs, .. File.ReadLines(StartupArgsPath).Where(arg => !arg.StartsWith('#'))];
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"An error occured while trying to merge arguments: {ex}", ConsoleColor.Red);
                return [.. startupArgs];
            }
        }

        private static void MigrateArgsFile()
        {
            const string obsoleteFile = "laargs.txt";

            try
            {
                if (!File.Exists(obsoleteFile))
                    return;

                if (string.IsNullOrWhiteSpace(File.ReadAllText(obsoleteFile)))
                {
                    File.Delete(obsoleteFile);
                    ConsoleUtil.WriteLine("Obsolete configuration file 'laargs.txt' is empty and has been deleted.", ConsoleColor.Gray);
                    return;
                }

                if (File.Exists(StartupArgsPath))
                    return;

                File.Move(obsoleteFile, StartupArgsPath);
                ConsoleUtil.WriteLine("Successfully migrated your old 'laargs' configuration.", ConsoleColor.DarkGreen);
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"An error occurred during migration: {ex}", ConsoleColor.Yellow);
            }
        }
    }
}
