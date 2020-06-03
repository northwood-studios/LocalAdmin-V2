using System;

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

        public static void Write(string content, ConsoleColor color = ConsoleColor.White, int height = 0)
        {
            lock (_lck)
            {
                content = content.Trim().Trim(ToTrim);
                content = string.IsNullOrEmpty(content) ? "" : $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {content}";

                Console.BackgroundColor = ConsoleColor.Black;

                try
                {
                    Console.ForegroundColor = color;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (height > 0)
                    Console.CursorTop += height;

                Console.Write(content);

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;

                //Logger.Log(content);
            }
        }

        public static void WriteLine(string content, ConsoleColor color = ConsoleColor.White, int height = 0)
        {
            lock (_lck)
            {
                content = content.Trim().Trim(ToTrim);
                content = string.IsNullOrEmpty(content) ? string.Empty : $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {content}";

                Console.BackgroundColor = ConsoleColor.Black;

                try
                {
                    Console.ForegroundColor = color;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (height > 0)
                    Console.CursorTop += height;

                Console.WriteLine(content);

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;

                //Logger.Log(content);
            }
        }
    }
}