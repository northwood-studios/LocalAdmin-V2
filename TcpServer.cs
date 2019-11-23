using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LocalAdmin.V2
{
    public class TcpServer
    {
        public event EventHandler<string> Received;
        
        private TcpListener listener;
        private TcpClient? client;
        private NetworkStream? networkStream;
        private StreamReader? streamReader;
        private StreamWriter? streamWriter;

        private Task? dataReaderTask;

        private volatile bool exit = false;

        public TcpServer(ushort port)
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
        }

        public void Start()
        {
            listener.Start();
            client = listener.AcceptTcpClient();
            networkStream = client.GetStream();
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);

            dataReaderTask = Task.Run(() =>
            {
                while (!exit)
                {
                    if (networkStream.DataAvailable)
                    {
                        Received?.Invoke(this, streamReader.ReadLine());
                    }
                }
            });
        }

        public void Stop()
        {
            exit = true;
            
            listener.Stop();
            client.Close();
            streamReader.Dispose();
            streamWriter.Dispose();
        }

        public void WriteLine(string input)
        {
            streamWriter.WriteLine(input);
        }
    }
}