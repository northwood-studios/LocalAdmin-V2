using System;
using System.Globalization;
using LocalAdmin.V2.IO.Logging;

namespace LocalAdmin.V2.IO
{
    public static class ConsoleUtil
    {
        private static readonly char[] ToTrim = { '\n', '\r' };

        private static readonly object Lck = new object();

        public static void Clear()
        {
            lock (Lck)
            {
                Console.Clear();
            }
        }
        
        private static string GetLogsLocalTimestamp() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}]";
        
        private static string GetLogsUtcTimestamp() => $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}Z]";
        
        public static string GetLogsTimestamp() => Core.LocalAdmin.Configuration != null && Core.LocalAdmin.Configuration!.LaLogsUseUtc
            ? GetLogsUtcTimestamp()
            : GetLogsLocalTimestamp();
        
        private static string GetLiveViewLocalTimestamp() => $"[{DateTime.Now.ToString(Core.LocalAdmin.Configuration!.LaLiveViewTimeFormat, CultureInfo.InvariantCulture)}]";
        
        private static string GetLiveViewUtcTimestamp() => $"[{DateTime.UtcNow.ToString(Core.LocalAdmin.Configuration!.LaLiveViewTimeUtcFormat, CultureInfo.InvariantCulture)}Z]";

        private static string GetLiveViewTimestamp() => Core.LocalAdmin.Configuration == null ? GetLogsLocalTimestamp() : Core.LocalAdmin.Configuration!.LaLiveViewUseUtc
            ? GetLiveViewUtcTimestamp()
            : GetLiveViewLocalTimestamp();

        public static void Write(string content, ConsoleColor color = ConsoleColor.White, int height = 0, bool log = true, bool display = true)
        {
            lock (Lck)
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
            lock (Lck)
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