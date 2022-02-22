using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LocalAdmin.V2.IO.ExitHandlers
{
    /// <summary>
    ///     Native signal processing on Windows NT systems.
    /// </summary>
    internal sealed class WindowsHandler : IExitHandler
    {
        public static readonly WindowsHandler Handler = new WindowsHandler();

        // .Net Core sometimes crashes when the delegate isn't in a field
        private static readonly HandlerRoutine Routine = OnNativeSignal;

        public void Setup()
        {
            if (!SetConsoleCtrlHandler(Routine, true))
            {
                throw new Win32Exception();
            }
        }

        private static bool OnNativeSignal(CtrlTypes ctrl)
        {
            if (Core.LocalAdmin.Singleton == null)
                return true;
            
            Core.LocalAdmin.Singleton.DisableExitActionSignals = true;
            Core.LocalAdmin.Singleton.ExitAction = Core.LocalAdmin.ShutdownAction.SilentShutdown;
            Core.LocalAdmin.Singleton.Exit(0);

            return true;
        }

        #region Native

        [DllImport("Kernel32", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        private delegate bool HandlerRoutine(CtrlTypes ctrlType);

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
