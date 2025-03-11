using System;
using System.Runtime.InteropServices;

namespace LocalAdmin.V2.IO.ExitHandlers
{
    /// <summary>
    ///     Native signal processing on Unix systems.
    /// </summary>
    internal sealed class UnixHandler : IExitHandler, IDisposable
    {
        public static readonly UnixHandler Handler = new();
        private PosixSignalRegistration[]? _signals;

        public void Setup()
        {
            Dispose();

            Action<PosixSignalContext> handler = SignalHandler;
            _signals =
            [
                PosixSignalRegistration.Create(PosixSignal.SIGINT, handler), // CTRL + C pressed
                PosixSignalRegistration.Create(PosixSignal.SIGTERM, handler), // Sending KILL
                PosixSignalRegistration.Create((PosixSignal)10, handler), // SIGUSR1
                PosixSignalRegistration.Create((PosixSignal)12, handler), // SIGUSR1
                PosixSignalRegistration.Create(PosixSignal.SIGHUP, handler), // Terminal is closed
                PosixSignalRegistration.Create(PosixSignal.SIGQUIT, handler) // QUIT pressed
            ];
        }

        private static void SignalHandler(PosixSignalContext obj)
        {
            if (Core.LocalAdmin.Singleton == null)
                Environment.Exit(0);
            else
                Core.LocalAdmin.HandleExitSignal();
        }

        public void Dispose()
        {
            foreach (PosixSignalRegistration signal in _signals.AsSpan())
            {
                signal.Dispose();
            }

            _signals = null;
        }
    }
}