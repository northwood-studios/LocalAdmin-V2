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
        /// Merges Command-line arguments and arguments in <paramref name="StartupArgsPath"/>
        /// </summary>
        /// <param name="CMDArgs">Runtime Command-Line Arguments</param>
        /// <returns>Merged Arguments</returns>
        public static string[] MergeStartupArgs(IEnumerable<string> CMDArgs)
        {
            try
            {
                List<string> StartupArgs = new List<string>();
                StartupArgs.AddRange(CMDArgs);

                if (!File.Exists(Path.GetFullPath(StartupArgsPath)))
                    File.WriteAllText(Path.GetFullPath(StartupArgsPath), string.Empty);

                foreach (string farg in File.ReadAllLines(Path.GetFullPath(StartupArgsPath)))
                {
                    if (farg.StartsWith("#"))
                        StartupArgs.Add(farg);
                }
                return StartupArgs.ToArray();
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"An error occured while trying to merge arguments :: {ex}", ConsoleColor.Red);
                return CMDArgs.ToArray();
            }
        }
    }
}
