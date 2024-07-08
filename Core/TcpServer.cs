using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.IO.Logging;

namespace LocalAdmin.V2.Core;

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
        ExitActionRestart = 0x16,
        Heartbeat = 0x17
    }

    public event EventHandler<string>? Received;

    private readonly TcpListener _listener;
    private TcpClient? _client;
    private NetworkStream? _networkStream;

    internal ushort ConsolePort;
    internal bool Connected;
    private bool _exit = true;
    private int _txBuffer;
    private readonly object _lck = new();
    private readonly UTF8Encoding _encoding = new(false, true);

    public TcpServer() => _listener = new TcpListener(IPAddress.Loopback, 0);

    public void Start()
    {
        lock (_lck)
        {
            _exit = false;

            _listener.Start();
            ConsolePort = (ushort)((IPEndPoint)_listener.LocalEndpoint).Port;
            _listener.BeginAcceptTcpClient(result =>
            {
                lock (_lck)
                {
                    if (_exit)
                        return;
                }

                _client = _listener.EndAcceptTcpClient(result);
                _client.NoDelay = true;

                _client.ReceiveBufferSize = LocalAdmin.Configuration!.SlToLaBufferSize;
                _client.SendBufferSize = LocalAdmin.Configuration.LaToSlBufferSize;

                _txBuffer = LocalAdmin.Configuration.LaToSlBufferSize;

                _networkStream = _client.GetStream();
                Connected = true;

                Task.Run(async () =>
                {
                    const int offset = sizeof(int);
                    var codeBuffer = new byte[1];
                    var lengthBuffer = new byte[offset];
                    var restartReceived = false;

                    while (true)
                    {
                        await Task.Delay(10);

                        lock (_lck)
                        {
                            if (_exit)
                                break;
                        }

                        if (_networkStream?.DataAvailable != true)
                            continue;

                        int readAmount = await _networkStream.ReadAsync(codeBuffer.AsMemory(0, 1));

                        if (readAmount == 0)
                            continue;

                        if (codeBuffer[0] < 16)
                        {
                            readAmount = await _networkStream.ReadAsync(lengthBuffer.AsMemory(0, offset));

                            if (readAmount < 4)
                            {
                                Received?.Invoke(this,
                                    "4[LocalAdmin] Received **INVALID** data message length. Length: " + readAmount);
                                continue;
                            }

                            var length = MemoryMarshal.Cast<byte, int>(lengthBuffer)[0];
                            var buffer = ArrayPool<byte>.Shared.Rent(length);

                            while (_client.Available < length)
                                await Task.Delay(20);

                            readAmount = await _networkStream.ReadAsync(buffer.AsMemory(0, length));

                            if (readAmount != length)
                            {
                                Received?.Invoke(this,
                                    $"4[LocalAdmin] Received **INVALID** data message. Length received: {readAmount}. Length expected: {length}.");
                                continue;
                            }

                            var message = $"{codeBuffer[0]:X}{_encoding.GetString(buffer, 0, length)}";
                            ArrayPool<byte>.Shared.Return(buffer);

                            Received?.Invoke(this, message);
                        }
                        else
                        {
                            if (LocalAdmin.PrintControlMessages)
                                Received?.Invoke(this, "7[LocalAdmin] Received control message: " + codeBuffer[0]);

                            switch ((OutputCodes)codeBuffer[0])
                            {
                                case OutputCodes.RoundRestart:
                                    if (restartReceived)
                                        Logger.Initialize();
                                    else restartReceived = true;
                                    break;

                                case OutputCodes.IdleEnter:
                                    LocalAdmin.Singleton?.SetIdleModeState(true);
                                    break;

                                case OutputCodes.IdleExit:
                                    LocalAdmin.Singleton?.SetIdleModeState(false);
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

                                case OutputCodes.Heartbeat:
                                    LocalAdmin.Singleton!.HandleHeartbeat();
                                    break;

                                default:
                                    Received?.Invoke(this,
                                        "4[LocalAdmin] Received **INVALID** control message: " + codeBuffer[0]);
                                    break;
                            }
                        }
                    }
                });
            }, _listener);
        }
    }

    public void Stop()
    {
        lock (_lck)
        {
            if (_exit)
                return;
            _exit = true;

            _listener.Stop();
            _client?.Close();
        }
    }

    public void WriteLine(string input)
    {
        lock (_lck)
        {
            if (_exit) return;
            const int offset = sizeof(int);

            var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(input.Length) + offset);

            var length = _encoding.GetBytes(input, 0, input.Length, buffer, offset);

            if (length + offset > _txBuffer)
            {
                ConsoleUtil.WriteLine("Failed to send command - configured LA to SL buffer size is too small. Please increase it in the LocalAdmin config file to run this command!", ConsoleColor.Red);
                ArrayPool<byte>.Shared.Return(buffer);
                return;
            }

            MemoryMarshal.Cast<byte, int>(buffer)[0] = length;

            _networkStream!.Write(buffer, 0, length + offset);
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}