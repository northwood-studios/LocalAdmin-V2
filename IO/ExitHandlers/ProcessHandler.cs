using System;
using System.Diagnostics;

namespace LocalAdmin.V2.IO.ExitHandlers;

internal sealed class ProcessHandler : IExitHandler
{
    public static readonly ProcessHandler Handler = new();

    public void Setup()
    {
        var process = Process.GetCurrentProcess();
        process.EnableRaisingEvents = true;
        process.Exited += Exit;
    }

    private static void Exit(object? sender, EventArgs e)
    {
        if (Core.LocalAdmin.Singleton == null)
            return;

        Core.LocalAdmin.Singleton.DisableExitActionSignals = true;
        Core.LocalAdmin.Singleton.ExitAction = Core.LocalAdmin.ShutdownAction.SilentShutdown;
        Core.LocalAdmin.Singleton.Exit(0);
    }
}