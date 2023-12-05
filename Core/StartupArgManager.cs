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
        private const string StartupArgsPath = "laargs.txt";

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

                foreach (string arg in File.ReadAllLines(StartupArgsPath))
                {
                    if (!arg.StartsWith("#", StringComparison.Ordinal))
                        startupArgs.Add(arg);
                }

                return startupArgs.ToArray();
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"An error occured while trying to merge arguments: {ex}", ConsoleColor.Red);
                return startupArgs.ToArray();
            }
        }
    }
}
