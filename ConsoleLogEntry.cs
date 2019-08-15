using System;
using System.Collections.Generic;
using System.Text;

namespace LocalAdmin.V2
{
	public class ConsoleLogEntry
	{
		public readonly string Content;
		public readonly ConsoleColor Color;
		public readonly int Height;

		public ConsoleLogEntry(string c, ConsoleColor co, int h)
		{
			Content = c;
			Color = co;
			Height = h;
		}
	}
}
