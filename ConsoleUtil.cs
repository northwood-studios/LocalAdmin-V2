using System;

namespace LocalAdmin.V2
{
    public static class ConsoleUtil
    {
        private static readonly char[] ToTrim = { '\n', '\r' };

        private static object _lck;

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

                Console.WriteLine(content);

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;

                Logger.Log(content);
            }
        }
	}
}