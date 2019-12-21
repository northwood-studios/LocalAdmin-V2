using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LocalAdmin.V2
{
    public class TcpServer
    {
        public event EventHandler<string>? Received;

        private TcpListener listener;
        private TcpClient? client;
        private NetworkStream? networkStream;
        private StreamReader? streamReader;

        private volatile bool exit = false;

        public TcpServer(ushort port)
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
        }

        public void Start()
        {
            exit = false;

            listener.Start();
            client = listener.AcceptTcpClient();
            networkStream = client.GetStream();
            streamReader = new StreamReader(networkStream);

            new Thread(() =>
            {
                while (!exit)
                {
                    if (!streamReader!.EndOfStream)
                    {
                        Received?.Invoke(this, streamReader!.ReadLine()!);
                    }

                    Thread.Sleep(10);
                }
            }).Start();
        }

        public void Stop()
        {
            if (!exit)
            {
                exit = true;

                listener.Stop();
                client!.Close();
                streamReader!.Dispose();
            }
        }

        public void WriteLine(string input)
        {
            if (!exit)
            {
                var buffer = Encoding.UTF8.GetBytes(input + Environment.NewLine);
                networkStream!.Write(buffer, 0, buffer.Length);
            }
        }
    }
}