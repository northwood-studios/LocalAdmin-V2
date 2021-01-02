using System;
using System.IO;

namespace LocalAdmin.V2.IO.Logging
{
    public static class Logger
    {
        private static StreamWriter? _sw;
        private static string? _logPath;
        internal const string LogFolderName = "LocalAdminLogs";
        
        public static void Initialize()
        {
            if (!Core.LocalAdmin.EnableLogging)
                return;
            if (_sw != null)
                EndLogging();
            
            string dir = Core.LocalAdmin.GameUserDataRoot + LogFolderName + Path.DirectorySeparatorChar + Core.LocalAdmin.GamePort + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _logPath = dir + $"LocalAdmin Log {DateTime.Now:yyyy-MM-dd HH.mm.ss}.txt";
            _sw = new StreamWriter(_logPath) {AutoFlush = Core.LocalAdmin.AutoFlush};
            
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

        private static void AppendLog(string text)
        {
            if (_sw == null) return;
            
            try
            {
                if (_sw.BaseStream.CanWrite) _sw.WriteLine(text);
                else
                {
                    try
                    {
                        _sw.Close();
                    }
                    catch
                    {
                        //Ignore
                    }
                
                    _sw = File.AppendText(_logPath!);
                    _sw.AutoFlush = Core.LocalAdmin.AutoFlush;
                    _sw.WriteLine(text);
                }
            }
            catch (Exception e)
            {
                Console.Write("Failed to write log: " + e.Message);
            }
        }
        
        public static void Log(string text) => AppendLog(text);

        public static void Log(object obj) => AppendLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {obj}");
    }
}