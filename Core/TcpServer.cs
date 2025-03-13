using LocalAdmin.V2.IO;
using LocalAdmin.V2.IO.Logging;
using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private TcpClient? _client;
    private NetworkStream? _networkStream;

    private volatile bool _exit = true;

    internal ushort ConsolePort;
    internal bool Connected;
    private int _txBuffer;
    private readonly Lock _lck = new();
    private readonly UTF8Encoding _encoding = new(false, true);

    public void Start()
    {
        lock (_lck)
        {
            _exit = false;

            _listener.Start();
            ConsolePort = (ushort)((IPEndPoint)_listener.LocalEndpoint).Port;
            _listener.BeginAcceptTcpClient(result =>
            {
                _client = _listener.EndAcceptTcpClient(result);

                if (_exit)
                {
                    _client.Close();
                    return;
                }

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

                    while (!_exit)
                    {
                        try
                        {
                            await Task.Delay(10);

                            await _networkStream.ReadExactlyAsync(codeBuffer.AsMemory());

                            if (codeBuffer[0] < 16)
                            {
                                await _networkStream.ReadExactlyAsync(lengthBuffer.AsMemory());

                                var length = MemoryMarshal.Read<int>(lengthBuffer);
                                var buffer = ArrayPool<byte>.Shared.Rent(length);

                                await _networkStream.ReadExactlyAsync(buffer.AsMemory(0, length));

                                var message = $"{codeBuffer[0]:X}{_encoding.GetString(buffer, 0, length)}";
                                ArrayPool<byte>.Shared.Return(buffer);

                                Received?.Invoke(this, message);
                            }
                            else
                            {
                                if (LocalAdmin.PrintControlMessages)
                                    Received?.Invoke(this, $"7[LocalAdmin] Received control message: {codeBuffer[0]}");

                                switch ((OutputCodes)codeBuffer[0])
                                {
                                    case OutputCodes.RoundRestart:
                                        if (restartReceived)
                                            Logger.Initialize();
                                        else
                                            restartReceived = true;
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
                                            $"4[LocalAdmin] Received **INVALID** control message: {codeBuffer[0]}");
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleUtil.WriteLine($"Failed to read a message from the game: {ex.Message}", ConsoleColor.Red);
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
            if (Interlocked.Exchange(ref _exit, true))
                return;

            _client?.Close();
            _listener.Stop();
        }
    }

    public void WriteLine(string input)
    {
        lock (_lck)
        {
            if (_exit)
                return;

            const int offset = sizeof(int);

            var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(input.Length) + offset);

            var length = _encoding.GetBytes(input.AsSpan(), buffer.AsSpan(offset));
            MemoryMarshal.Write(buffer, length);

            length += offset;

            if (length > _txBuffer)
            {
                ConsoleUtil.WriteLine("Failed to send command - configured LA to SL buffer size is too small. Please increase it in the LocalAdmin config file to run this command!", ConsoleColor.Red);
            }
            else
            {
                _networkStream!.Write(buffer);
            }

            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}