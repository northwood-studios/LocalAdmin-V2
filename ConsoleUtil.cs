using System;

namespace LocalAdmin_V2_Net_Core
{
    public static class ConsoleUtil
    {
	    public static object WriteLock;

        public static void Write(string content, ConsoleColor color = ConsoleColor.White, int height = 0)
        {
	        content = content.TrimEnd('\n').TrimEnd('\r');
	        lock (WriteLock)
	        {
		        Console.CursorTop += height;
		        Console.ForegroundColor = color;

		        var time = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] ";

		        Console.WriteLine(content == "" ? "" : time + content);
		        Logger.Log(content == "" ? "" : time + content);

		        Console.ForegroundColor = ConsoleColor.White;
		        Console.BackgroundColor = ConsoleColor.Black;
	        }
        }
	}
}