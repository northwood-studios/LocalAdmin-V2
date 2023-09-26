using System;
using System.Collections.Generic;
using System.IO;

namespace LocalAdmin.V2.Core
{
    public sealed class StartupArgManager
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
        public static string[] MergeStartupArgs(string[] CMDArgs)
        {
            List<string> StartupArgs = new List<string>();
            foreach (string arg in CMDArgs)
            {
                if (!StartupArgs.Contains(arg))
                    StartupArgs.Add(arg);
            }

            foreach (string farg in File.ReadAllLines(Path.GetFullPath(StartupArgsPath)))
            {
                if (!farg.StartsWith("#") && !farg.StartsWith(" "))
                {
                    if (!StartupArgs.Contains(farg))
                        StartupArgs.Add(farg);
                }
            }

            return StartupArgs.ToArray();
        }
    }
}
