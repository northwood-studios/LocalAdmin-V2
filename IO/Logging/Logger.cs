﻿using System;
using System.IO;
using System.Text;

namespace LocalAdmin.V2.IO.Logging
{
    public static class Logger
    {
        private static StringBuilder? _sb;
        private static bool _logging;
        private static string? _logPath;
        internal const string LogFolderName = "LocalAdminLogs";
        
        public static void Initialize()
        {
            if (!Core.LocalAdmin.EnableLogging)
                return;
            if (_logging)
                EndLogging();
            
            string dir = Core.LocalAdmin.LaLogsPath ?? Core.LocalAdmin.GameUserDataRoot + LogFolderName + Path.DirectorySeparatorChar + Core.LocalAdmin.GamePort + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _logPath = dir + $"LocalAdmin Log {DateTime.Now:yyyy-MM-dd HH.mm.ss}.txt";
            _logging = true;

            Log($"{ConsoleUtil.GetLogsTimestamp()} Logging started.");
            Log($"{ConsoleUtil.GetLogsTimestamp()} Timezone offset: {DateTimeOffset.Now:zzz}");
        }

        public static void EndLogging()
        {
            if (!_logging) return;
            Log($"{ConsoleUtil.GetLogsTimestamp()} --- END OF LOG ---", true);

            _logging = false;
            _sb = null;
        }

        private static void AppendLog(string text, bool flush = false)
        {
            if (!_logging) return;
            
            try
            {
                if (Core.LocalAdmin.AutoFlush)
                    File.AppendAllText(_logPath!, text + Environment.NewLine);
                else
                {
                    _sb ??= new StringBuilder();
                    _sb.AppendLine(text);

                    if (_sb.Length <= 1000 || flush) return;
                    File.AppendAllText(_logPath!, _sb.ToString());
                    _sb.Clear();
                }
            }
            catch (Exception e)
            {
                Console.Write("Failed to write log: " + e.Message);
            }
        }
        
        public static void Log(string text, bool flush = false) => AppendLog(text, flush);

        public static void Log(object obj, bool flush = false) => AppendLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {obj}", flush);
    }
}