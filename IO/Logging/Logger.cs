using System;
using System.IO;
using System.Text;

namespace LocalAdmin.V2.IO.Logging
{
    public static class Logger
    {
        private static StringBuilder? _sb;
        private static bool _logging;
        private static string? _logPath;
        private static ulong _totalLength, _totalEntries;
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

            _totalLength = 0;
            _totalEntries = 0;
            _logPath = dir + $"LocalAdmin Log {DateTime.Now:yyyy-MM-dd HH.mm.ss}.txt";
            _logging = true;

            Log($"{ConsoleUtil.GetLogsTimestamp()} Logging started.");
            Log($"{ConsoleUtil.GetLogsTimestamp()} Timezone offset: {DateTimeOffset.Now:zzz}");
        }

        public static void EndLogging(bool bypass = false)
        {
            if (!_logging && !bypass) return;
            AppendLog($"{ConsoleUtil.GetLogsTimestamp()} --- END OF LOG ---", true, true);

            _logging = false;
            _sb = null;
        }

        private static void AppendLog(string text, bool flush = false, bool bypass = false)
        {
            if (!_logging && !bypass) return;
            
            try
            {
                if (Core.LocalAdmin.AutoFlush)
                    File.AppendAllText(_logPath!, text + Environment.NewLine);
                else
                {
                    _sb ??= new StringBuilder();
                    _sb.AppendLine(text);

                    if (_sb.Length > 1000 || flush)
                    {
                        File.AppendAllText(_logPath!, _sb.ToString());
                        _sb.Clear();
                    }
                }

                if (bypass)
                    return;
                
                _totalEntries++;
                _totalLength += (uint)text.Length;

                if (_totalEntries > Core.LocalAdmin.LogEntriesLimit && Core.LocalAdmin.LogEntriesLimit > 0)
                {
                    _logging = false;
                    AppendLog("Log entries limit exceeded. Logging Stopped.", bypass: true);
                    EndLogging(true);
                }
                
                if (_totalLength > Core.LocalAdmin.LogLengthLimit && Core.LocalAdmin.LogLengthLimit > 0)
                {
                    _logging = false;
                    AppendLog("Log length limit exceeded. Logging Stopped.", bypass: true);
                    EndLogging(true);
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