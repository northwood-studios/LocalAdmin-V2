using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace LocalAdmin.V2
{
    public static class ConsoleUtil
    {
	    private static bool _exit;
		private static Queue<ConsoleLogEntry> _q;
	    private static readonly char[] ToTrim = {'\n', '\r'};
	    private static Thread _queueThread;

	    public static void Init()
	    {
			_q = new Queue<ConsoleLogEntry>();
			_queueThread = new Thread(ProcessQueue) {IsBackground = true};
			_queueThread.Start();
	    }

        public static void Write(string content, ConsoleColor color = ConsoleColor.White, int height = 0)
        {
	        content = content.Trim().Trim(ToTrim);
			_q.Enqueue(new ConsoleLogEntry(content == "" ? "" : $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {content}", color, height));
        }

        private static void ProcessQueue()
        {
			while (_q == null)
				Thread.Sleep(25);

	        while (!_exit || _q.Count > 0)
	        {
		        if (_q.Count == 0)
		        {
					Thread.Sleep(20);
					continue;
		        }

		        try
		        {
			        var entry = _q.Dequeue();

			        Console.BackgroundColor = ConsoleColor.Black;

			        try
			        {
				        Console.ForegroundColor = entry.Color;
			        }
			        catch
			        {
				        Console.ForegroundColor = ConsoleColor.White;
			        }

			        if (entry.Height > 0)
				        Console.CursorTop += entry.Height;

			        Console.WriteLine(entry.Content);

			        Console.ForegroundColor = ConsoleColor.White;
			        Console.BackgroundColor = ConsoleColor.Black;

			        Logger.Log(entry.Content);
		        }
		        catch (Exception e)
		        {
					Write("[LocalAdmin] Queue processing exception: " + e.Message, ConsoleColor.Red);
		        }
	        }
        }

        public static void Terminate()
        {
	        _exit = true;
        }
	}
}