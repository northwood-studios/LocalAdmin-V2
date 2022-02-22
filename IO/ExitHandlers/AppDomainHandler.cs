using System;

namespace LocalAdmin.V2.IO.ExitHandlers
{
    internal sealed class AppDomainHandler : IExitHandler
    {
        public static readonly AppDomainHandler Handler = new AppDomainHandler();

        public void Setup()
        {
            AppDomain.CurrentDomain.ProcessExit += Exit;
            AppDomain.CurrentDomain.DomainUnload += DomainUnload;
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        }

        private static void Exit(object? sender, EventArgs e)
        {
            if (Core.LocalAdmin.Singleton != null)
                Core.LocalAdmin.Singleton.Exit(0);
        }

        private static void DomainUnload(object? sender, EventArgs e)
        {
            if (Core.LocalAdmin.Singleton == null)
                return;
            
            Core.LocalAdmin.Singleton.DisableExitActionSignals = true;
            Core.LocalAdmin.Singleton.ExitAction = Core.LocalAdmin.ShutdownAction.SilentShutdown;
            Core.LocalAdmin.Singleton.Exit(0);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
                return;
            
            if (e.ExceptionObject is Exception ex)
            {
                ConsoleUtil.WriteLine($"Unhandled Exception: {ex}", ConsoleColor.Red);
                    
                if (Core.LocalAdmin.Singleton != null)
                    Core.LocalAdmin.Singleton.Exit(ex.HResult);
            }
            else
            {
                ConsoleUtil.WriteLine("Unhandled Exception!", ConsoleColor.Red);
                    
                if (Core.LocalAdmin.Singleton != null)
                    Core.LocalAdmin.Singleton.Exit(1);
            }
        }
    }
}
