using System;

namespace LocalAdmin.V2.IO.ExitHandlers
{
    internal sealed class AppDomainHandler : IExitHandler
    {
        public static readonly AppDomainHandler Handler = new AppDomainHandler();

        public void Setup()
        {
            AppDomain.CurrentDomain.ProcessExit += Exit;
            AppDomain.CurrentDomain.DomainUnload += DomandUnload;
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        }

        private static void Exit(object? sender, EventArgs e)
        {
            Core.LocalAdmin.Singleton.Exit(0);
        }

        private static void DomandUnload(object? sender, EventArgs e)
        {
            Core.LocalAdmin.Singleton.Exit(0);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                if (e.ExceptionObject is Exception ex)
                {
                    ConsoleUtil.WriteLine($"Unhandled Exception: {ex}", ConsoleColor.Red);
                    Core.LocalAdmin.Singleton.Exit(ex.HResult);
                }
                else
                {
                    ConsoleUtil.WriteLine("Unhandled Exception!", ConsoleColor.Red);
                    Core.LocalAdmin.Singleton.Exit(1);
                }
            }
        }
    }
}
