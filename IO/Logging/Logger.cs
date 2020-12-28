using System;
using System.IO;

namespace LocalAdmin.V2.IO.Logging
{
    public static class Logger
    {
        private static StreamWriter? _sw;
        internal const string LogFolderName = "LocalAdminLogs";
        
        public static void Initialize()
        {
            if (!Core.LocalAdmin.EnableLogging)
                return;
            if (_sw != null)
                EndLogging();
            
            string dir = Core.LocalAdmin.GameUserDataRoot + LogFolderName + Path.DirectorySeparatorChar + Core.LocalAdmin.Singleton.GamePort + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _sw = new StreamWriter(dir +
                                   $"LocalAdmin Log {DateTime.Now:yyyy-MM-dd HH.mm.ss}.txt") {AutoFlush = Core.LocalAdmin.AutoFlush};
            
            Log($"{ConsoleUtil.GetTimestamp()} Logging started.");
        }

        public static void EndLogging()
        {
            if (_sw == null) return;
            Log($"{ConsoleUtil.GetTimestamp()} --- END OF LOG ---");

            _sw.Close();
            _sw.Dispose();
            _sw = null;
        }
        
        public static void Log(string text) => _sw?.WriteLine(text);

        public static void Log(object obj) => _sw?.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {obj}");
    }
}