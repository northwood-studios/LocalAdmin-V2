using System.Runtime.InteropServices;

namespace LocalAdmin.V2.NativeExitSignal
{
    public static class NativeSignalHandler
    {
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT,
            CTRL_SHUTDOWN_EVENT

        }

        public static void Setup()
        {
            SetConsoleCtrlHandler(OnNativeSignal, true);
        }

        private static bool OnNativeSignal(CtrlTypes ctrl)
        {
            Program.localAdmin!.Exit(1);
            return true;
        }
    }
}
