using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LocalAdmin.V2.IO.Logging;

namespace LocalAdmin.V2.Core
{
    public class TcpServer
    {
        private enum OutputCodes : byte
        {
            //0x00 - 0x0F - reserved for colors
	
            RoundRestart = 0x10,
            IdleEnter = 0x11,
            IdleExit = 0x12,
            ExitActionReset = 0x13,
            ExitActionShutdown = 0x14,
            ExitActionSilentShutdown = 0x15,
            ExitActionRestart = 0x16
        }
        
        public event EventHandler<string>? Received;

        private readonly TcpListener listener;
        private TcpClient? client;
        private NetworkStream? networkStream;

        internal ushort ConsolePort;
        internal bool Connected;
        private bool exit = true;
        private readonly object lck = new object();
        private readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

        public TcpServer() => listener = new TcpListener(IPAddress.Loopback, 0);

        public void Start()
        {
            lock (lck)
            {
                exit = false;

                listener.Start();
                ConsolePort = (ushort)((IPEndPoint)(listener.LocalEndpoint)).Port;
                listener.BeginAcceptTcpClient(result =>
                {
                    lock (lck)
                    {
                        if (exit)
                            return;
                    }

                    client = listener.EndAcceptTcpClient(result);
                    networkStream = client.GetStream();
                    Connected = true;

                    Task.Run(async () =>
                    {
                        const int offset = sizeof(int);
                        var codeBuffer = new byte[1];
                        var lengthBuffer = new byte[offset];
                        bool restartReceived = false;
                        while (true)
                        {
                            lock (lck)
                            {
                                if (exit)
                                    break;
                            }

                            if (networkStream?.DataAvailable == true)
                            {
                                await networkStream.ReadAsync(codeBuffer, 0, 1);

                                if (codeBuffer[0] < 16)
                                {
                                    await networkStream.ReadAsync(lengthBuffer, 0, offset);
                                    var length = (lengthBuffer[0] << 24) | (lengthBuffer[1] << 16) |
                                                 (lengthBuffer[2] << 8) | lengthBuffer[3];

                                    var buffer = ArrayPool<byte>.Shared.Rent(length);
                                    await networkStream.ReadAsync(buffer, 0, length);
                                    var message = $"{codeBuffer[0]:X}{encoding.GetString(buffer, 0, length)}";
                                    ArrayPool<byte>.Shared.Return(buffer);

                                    Received?.Invoke(this, message);
                                }
                                else
                                {
                                    if (LocalAdmin.PrintControlMessages)
                                        Received?.Invoke(this, "7[LocalAdmin] Received control message: " + codeBuffer[0]);

                                    switch ((OutputCodes) codeBuffer[0])
                                    {
                                        case OutputCodes.RoundRestart:
                                            if (restartReceived)
                                                Logger.Initialize();
                                            else restartReceived = true;
                                            break;

                                        case OutputCodes.IdleEnter:
                                            Console.Title = "[IDLE] " + LocalAdmin.BaseWindowTitle;
                                            break;

                                        case OutputCodes.IdleExit:
                                            Console.Title = LocalAdmin.BaseWindowTitle;
                                            break;

                                        case OutputCodes.ExitActionReset:
                                            if (!LocalAdmin.Singleton!.DisableExitActionSignals)
                                                LocalAdmin.Singleton.ExitAction = LocalAdmin.ShutdownAction.Crash;
                                            break;
                                        
                                        case OutputCodes.ExitActionShutdown:
                                            if (!LocalAdmin.Singleton!.DisableExitActionSignals)
                                                LocalAdmin.Singleton.ExitAction = LocalAdmin.ShutdownAction.Shutdown;
                                            break;
                                        
                                        case OutputCodes.ExitActionSilentShutdown:
                                            if (!LocalAdmin.Singleton!.DisableExitActionSignals)
                                                LocalAdmin.Singleton.ExitAction = LocalAdmin.ShutdownAction.SilentShutdown;
                                            break;
                                        
                                        case OutputCodes.ExitActionRestart:
                                            if (!LocalAdmin.Singleton!.DisableExitActionSignals)
                                                LocalAdmin.Singleton.ExitAction = LocalAdmin.ShutdownAction.Restart;
                                            break;
                                        
                                        default:
                                            Received?.Invoke(this, "4[LocalAdmin] Received **INVALID** control message: " + codeBuffer[0]);
                                            break;
                                    }
                                }
                            }

                            await Task.Delay(10);
                        }
                    });
                }, listener);
            }
        }

        public void Stop()
        {
            lock (lck)
            {
                if (exit) 
                    return;
                exit = true;

                listener.Stop();
                client?.Close();
            }
        }

        public void WriteLine(string input)
        {
            lock (lck)
            {
                if (exit) return;
                const int offset = sizeof(int);

                var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(input.Length) + offset);

                var length = encoding.GetBytes(input, 0, input.Length, buffer, offset);
                MemoryMarshal.Cast<byte, int>(buffer)[0] = length;

                networkStream!.Write(buffer, 0, length + offset);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
