using System;
using System.Diagnostics;

namespace LocalAdmin.V2.IO.ExitHandlers
{
	internal sealed class ProcessHandler : IExitHandler
	{
		public static readonly ProcessHandler Handler = new ProcessHandler();

		public void Setup()
		{
			Process.GetCurrentProcess().Exited += Exit;
		}

		private static void Exit(object? sender, EventArgs e)
		{
			Core.LocalAdmin.Singleton.Exit(0);
		}
	}
}
