﻿using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LocalAdmin.V2.IO.NativeSignalHandlers
{
    /// <summary>
    ///     Native signal processing on Windows NT systems.
    /// </summary>
    internal sealed class WindowsNTHandler : INativeSignalHandler
    {
        public static readonly WindowsNTHandler Handler = new WindowsNTHandler();

        public void Setup()
        {
            if (!SetConsoleCtrlHandler(OnNativeSignal, true))
            {
                throw new Win32Exception();
            }
        }

        private bool OnNativeSignal(CtrlTypes ctrl)
        {
            Core.LocalAdmin.Singleton.Exit(0);
            return true;
        }

        #region Native

        [DllImport("Kernel32", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        private enum CtrlTypes
        {
            CTRL_C_EVENT,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion
    }
}
