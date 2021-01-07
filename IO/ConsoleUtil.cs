using System;
using LocalAdmin.V2.IO.Logging;

namespace LocalAdmin.V2.IO
{
    public static class ConsoleUtil
    {
        private static readonly char[] ToTrim = { '\n', '\r' };

        private static object _lck = new object();

        public static void Clear()
        {
            lock (_lck)
            {
                Console.Clear();
            }
        }
        
        private static string GetLogsLocalTimestamp() => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}]";
        
        private static string GetLogsUtcTimestamp() => $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}";
        
        public static string GetLogsTimestamp() => Core.LocalAdmin.Configuration != null && Core.LocalAdmin.Configuration!.LaLogsUseUtc
            ? GetLogsUtcTimestamp()
            : GetLogsLocalTimestamp();
        
        private static string GetLiveViewLocalTimestamp() => $"[{DateTime.Now.ToString(Core.LocalAdmin.Configuration!.LaLiveViewTimeFormat)}]";
        
        private static string GetLiveViewUtcTimestamp() => $"[{DateTime.UtcNow.ToString(Core.LocalAdmin.Configuration!.LaLiveViewTimeUtcFormat)}Z]";

        private static string GetLiveViewTimestamp() => Core.LocalAdmin.Configuration == null ? GetLogsLocalTimestamp() : Core.LocalAdmin.Configuration!.LaLiveViewUseUtc
            ? GetLiveViewUtcTimestamp()
            : GetLiveViewLocalTimestamp();

        public static void Write(string content, ConsoleColor color = ConsoleColor.White, int height = 0, bool log = true, bool display = true)
        {
            lock (_lck)
            {
                content = string.IsNullOrEmpty(content) ? string.Empty : content.Trim().Trim(ToTrim);

                if (display)
                {
                    Console.BackgroundColor = ConsoleColor.Black;

                    try
                    {
                        Console.ForegroundColor = color;
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    if (height > 0 && !Core.LocalAdmin.NoSetCursor)
                        Console.CursorTop += height;

                    Console.Write($"{GetLiveViewTimestamp()} {content}");

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (log)
                    Logger.Log($"{GetLogsTimestamp()} {content}");
            }
        }

        public static void WriteLine(string content, ConsoleColor color = ConsoleColor.White, int height = 0, bool log = true, bool display = true)
        {
            lock (_lck)
            {
                content = string.IsNullOrEmpty(content) ? string.Empty : content.Trim().Trim(ToTrim);

                if (display)
                {
                    Console.BackgroundColor = ConsoleColor.Black;

                    try
                    {
                        Console.ForegroundColor = color;
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    if (height > 0 && !Core.LocalAdmin.NoSetCursor)
                        Console.CursorTop += height;

                    Console.WriteLine($"{GetLiveViewTimestamp()} {content}");

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (log)
                    Logger.Log($"{GetLogsTimestamp()} {content}");
            }
        }
    }
}