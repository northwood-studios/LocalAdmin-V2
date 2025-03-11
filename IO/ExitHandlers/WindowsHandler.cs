using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LocalAdmin.V2.IO.ExitHandlers;

/// <summary>
///     Native signal processing on Windows NT systems.
/// </summary>
internal sealed unsafe partial class WindowsHandler : IExitHandler
{
    public static readonly WindowsHandler Handler = new();

    public void Setup()
    {
        if (SetConsoleCtrlHandler(&OnNativeSignal, 1) != 0)
        {
            throw new Win32Exception();
        }
    }

    [UnmanagedCallersOnly]
    private static int OnNativeSignal(CtrlTypes ctrl)
    {
        if (Core.LocalAdmin.Singleton == null)
            Environment.Exit(0);
        else
            Core.LocalAdmin.HandleExitSignal();

        return 1;
    }

    #region Native
    [LibraryImport("Kernel32", SetLastError = true)]
    private static partial int SetConsoleCtrlHandler(delegate* unmanaged<CtrlTypes, int> handler, int add);

    private enum CtrlTypes : uint
    {
        CtrlCEvent = 0,
        CtrlBreakEvent = 1,
        CtrlCloseEvent = 2,
        CtrlLogoffEvent = 5,
        CtrlShutdownEvent = 6
    }
    #endregion
}