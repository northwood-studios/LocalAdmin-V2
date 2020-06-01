using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LocalAdmin.V2.Core
{
    public class TcpServer
    {
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
                ConsolePort = (ushort) ((IPEndPoint) (listener.LocalEndpoint)).Port;
                listener.BeginAcceptTcpClient(result =>
                {
                    client = listener.EndAcceptTcpClient(result);
                    networkStream = client.GetStream();
                    Connected = true;

                    Task.Run(async () =>
                    {
                        const int offset = sizeof(int);
                        var lengthBuffer = new byte[offset];
                        while (true)
                        {
                            lock (lck)
                            {
                                if (exit)
                                    break;
                            }

                            if (networkStream?.DataAvailable == true)
                            {
                                await networkStream.ReadAsync(lengthBuffer, 0, offset);
                                var length = MemoryMarshal.Cast<byte, int>(lengthBuffer)[0];

                                var buffer = ArrayPool<byte>.Shared.Rent(length);
                                await networkStream.ReadAsync(buffer, 0, length);
                                var message = encoding.GetString(buffer, 0, length);
                                ArrayPool<byte>.Shared.Return(buffer);

                                Received?.Invoke(this, message);
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
                if (exit) return;
                exit = true;

                listener.Stop();
                client!.Close();
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