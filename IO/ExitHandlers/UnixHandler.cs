#if LINUX_SIGNALS
using Mono.Unix;
using Mono.Unix.Native;
using System.Threading;

namespace LocalAdmin.V2.IO.ExitHandlers
{
    /// <summary>
    ///     Native signal processing on Unix systems.
    /// </summary>
    internal sealed class UnixHandler : IExitHandler
    {
        public static readonly UnixHandler Handler = new UnixHandler();
        private static readonly UnixSignal[] Signals = {
            new UnixSignal(Signum.SIGINT),  // CTRL + C pressed
            new UnixSignal(Signum.SIGTERM), // Sending KILL
            new UnixSignal(Signum.SIGUSR1),
            new UnixSignal(Signum.SIGUSR2),
            new UnixSignal(Signum.SIGHUP)   // Terminal is closed
        };


        public void Setup()
        {
            new Thread(() =>
            {
                // Blocking operation with infinite expectation of any signal
                UnixSignal.WaitAny(Signals, -1);
                Core.LocalAdmin.Singleton.Exit(0);
            }).Start();
        }
    }
}
#endif
