using System;
using LocalAdmin.V2.IO.Logging;

namespace LocalAdmin.V2.IO
{
    public static class ConsoleUtil
    {
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
                Console.ResetColor();
                
                Console.Write($"[{DateTime.Now:HH:mm:ss.fff}] ");

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

                Console.Write(content);

                Console.ResetColor();

                Logger.Log(content);
            }
        }

        public static void WriteLine(string content, ConsoleColor color = ConsoleColor.White, int height = 0)
        {
            Write(content + Environment.NewLine, color, height);
        }
    }
}