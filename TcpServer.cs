using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalAdmin.V2
{
    public class TcpServer
    {
        public event EventHandler<string>? Received;

        private readonly TcpListener listener;
        private TcpClient? client;
        private NetworkStream? networkStream;
        private StreamReader? streamReader;

        internal volatile bool Connected;
        private volatile bool exit = true;
        private readonly object lck = new object();

        public TcpServer(ushort port)
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
        }

        public void Start()
        {
            lock (lck)
            {
                exit = false;

                listener.Start();
                listener.BeginAcceptTcpClient(new AsyncCallback((result) =>
                {
                    client = listener.EndAcceptTcpClient(result);
                    networkStream = client.GetStream();
                    Connected = true;
                    streamReader = new StreamReader(networkStream);

                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            lock (lck)
                            {
                                if (exit)
                                    break;
                            }

                            if (!streamReader?.EndOfStream == true)
                            {
                                Received?.Invoke(this, streamReader!.ReadLine()!);
                            }

                            await Task.Delay(10);
                        }
                    });
                }), listener);
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
                streamReader!.Dispose();
            }
        }

        public void WriteLine(string input)
        {
            lock (lck)
            {
                if (exit) return;
                var buffer = Encoding.UTF8.GetBytes(input + Environment.NewLine);
                networkStream!.Write(buffer, 0, buffer.Length);
            }
        }
    }
}