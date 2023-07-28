using LocalAdmin.V2.Commands;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.IO.ExitHandlers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalAdmin.V2.Commands.PluginManager;
using LocalAdmin.V2.IO.Logging;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Core;
/*
    * Console colors:
    * Gray - LocalAdmin log
    * Red - critical error
    * DarkGray - insignificant info
    * Cyan - Header or important tip
    * Yellow - warning
    * DarkGreen - success
    * Blue - normal SCPSL log
*/
public sealed class LocalAdmin : IDisposable
{
    public const string VersionString = "2.5.11";

    public static ushort DefaultPort = 7777;
    private static readonly ConcurrentQueue<string> InputQueue = new();
    private static readonly Stopwatch RestartsStopwatch = new();
    private static string? _previousPat;
    private static bool _firstRun = true;
    private static string _gameArguments = string.Empty;
    private static bool _exit, _processRefreshFail;
    private static bool _noTrueColor;
    private static bool _stdPrint;
    private static bool _ignoreNextRestart;
    private static int _restarts = -1, _restartsLimit = 4, _restartsTimeWindow = 480; //480 seconds = 8 minutes

    private readonly CommandService _commandService = new();
    private readonly string _scpslExecutable;
    private volatile bool _processClosing;
    private uint _heartbeatSpanMaxThreshold;
    private uint _heartbeatRestartInSeconds;
    private Process? _gameProcess;
    private Task? _readerTask, _heartbeatMonitoringTask;

    internal static readonly Stopwatch HeartbeatStopwatch = new();
    internal static LocalAdmin? Singleton;
    internal static ushort GamePort;
    internal static string? ConfigPath, CurrentConfigPath, LaLogsPath, GameLogsPath;
    internal static ulong LogLengthLimit = 25000000000, LogEntriesLimit = 10000000000;
    internal static Config? Configuration;
    internal static DataJson? DataJson;
    internal static string BaseWindowTitle = $"LocalAdmin v. {VersionString}";
    internal static bool NoSetCursor, PrintControlMessages, AutoFlush = true, EnableLogging = true, NoPadding, DismissPluginsSecurityWarning;

    internal ShutdownAction ExitAction = ShutdownAction.Crash;
    internal bool DisableExitActionSignals;
    internal HeartbeatStatus CurrentHeartbeatStatus = HeartbeatStatus.Disabled;
    internal uint HeartbeatWarningStage;

    internal TcpServer? Server { get; private set; }
    internal bool EnableGameHeartbeat { get; private set; }

    internal enum ShutdownAction : byte
    {
        Crash,
        Shutdown,
        SilentShutdown,
        Restart
    }

    private enum CaptureArgs : byte
    {
        None,
        ArgsPassthrough,
        ConfigPath,
        LaLogsPath,
        GameLogsPath,
        RestartsLimit,
        RestartsTimeWindow,
        LogLengthLimit,
        LogEntriesLimit
    }

    internal enum HeartbeatStatus : byte
    {
        Disabled,
        AwaitingFirstHeartbeat,
        Active
    }

    internal LocalAdmin()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
            _scpslExecutable = "SCPSL.exe";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
            _scpslExecutable = "SCPSL.x86_64";
        }
        else
        {
            ConsoleUtil.WriteLine("Failed - Unsupported platform! Please switch to the Windows, or Linux platform to continue.", ConsoleColor.Red);
            // shut up dotnet
            _scpslExecutable = string.Empty;
            Exit(1);
        }
    }

    internal async Task Start(string[] args)
    {
        Singleton = this;
        Console.Title = BaseWindowTitle;

        HeartbeatStopwatch.Reset();

        if (!PathManager.CorrectPathFound && !args.Contains("--skipHomeCheck", StringComparer.Ordinal))
        {
            ConsoleUtil.WriteLine($"Welcome to LocalAdmin version {VersionString}!", ConsoleColor.Red);
            ConsoleUtil.WriteLine("Can't obtain a valid user home directory path!", ConsoleColor.Red);
            //If statements will expose future performace issues. switching "CurrentPlatform" (string) will reduce the startup time.
            if (OperatingSystem.IsWindows())
            {
                ConsoleUtil.WriteLine("Such error should never occur on Windows.", ConsoleColor.Red);
                ConsoleUtil.WriteLine("Open issue on the LocalAdmin GitHub repository (https://github.com/northwood-studios/LocalAdmin-V2/issues) or contact our technical support!", ConsoleColor.Red);
            }else if (OperatingSystem.IsLinux())
            {
                ConsoleUtil.WriteLine("Make sure to export a valid path, for example using command: export HOME=/home/username-here", ConsoleColor.Red);
                ConsoleUtil.WriteLine("You may want to add that command to the top of ~/.bashrc file and restart the terminal session to avoid having to enter that command every time.", ConsoleColor.Red);
            }
            else
            {
                ConsoleUtil.WriteLine("You are running LocalAdmin on an unsupported platform, please switch to Windows or Linux!", ConsoleColor.Red);
                throw new PlatformNotSupportedException();
            }
            ConsoleUtil.WriteLine("To skip this check, use --skipHomeCheck argument.", ConsoleColor.Red);
            Terminate();
            return;
        }

        if (_restartsLimit > -1)
        {
            if (_restartsTimeWindow > 0)
            {
                if (!RestartsStopwatch.IsRunning)
                    RestartsStopwatch.Start();
                else if (RestartsStopwatch.Elapsed.TotalSeconds > _restartsTimeWindow)
                {
                    RestartsStopwatch.Restart();
                    _restarts = 0;
                }
            }

            if (!_ignoreNextRestart)
                _restarts++;
            else _ignoreNextRestart = false;

            if (_restarts > _restartsLimit)
            {
                ConsoleUtil.WriteLine("Restarts limit exceeded.", ConsoleColor.Red);
                Terminate();
            }
        }

        try
        {
            await LoadJsonOrTerminate();

            if (DataJson!.EulaAccepted == null)
            {
                ConsoleUtil.WriteLine($"Welcome to LocalAdmin version {VersionString}!", ConsoleColor.Cyan);
                ConsoleUtil.WriteLine("Before starting please read and accept the SCP:SL EULA.", ConsoleColor.Cyan);
                ConsoleUtil.WriteLine("You can find it on the following website: https://link.scpslgame.com/eula", ConsoleColor.Cyan);
                ConsoleUtil.WriteLine("", ConsoleColor.Cyan);
                ConsoleUtil.WriteLine("Do you accept the EULA? [yes/no]", ConsoleColor.Cyan);

                ReadInput((input) =>
                    {
                        if (input == null)
                            return false;

                        switch (input.ToLowerInvariant())
                        {
                            case "y":
                            case "yes":
                            case "1":
                                DataJson.EulaAccepted = DateTime.UtcNow;
                                return true;

                            case "n":
                            case "no":
                            case "nope":
                            case "0":
                                ConsoleUtil.WriteLine("You have to accept the EULA to use LocalAdmin and SCP: Secret Laboratory Dedicated Server.", ConsoleColor.Red);
                                Terminate();
                                return true;

                            default:
                                return false;
                        }

                    }, () => { },
                    () =>
                    {
                        ConsoleUtil.WriteLine("Do you accept the EULA? [yes/no]", ConsoleColor.Red);
                    });

                if (!_exit)
                    await SaveJsonOrTerminate();
            }

            var reconfigure = false;
            var useDefault = false;

            if (_firstRun)
            {
                if (args.Length == 0 || !ushort.TryParse(args[0], out GamePort))
                {
                    ConsoleUtil.WriteLine("You can pass port number as first startup argument.",
                        ConsoleColor.Green);
                    Console.WriteLine(string.Empty);
                    ConsoleUtil.Write($"Port number (default: {DefaultPort}): ", ConsoleColor.Green);

                    ReadInput((input) =>
                        {
                            if (!string.IsNullOrEmpty(input))
                                return ushort.TryParse(input, out GamePort);
                            GamePort = DefaultPort;
                            return true;

                        }, () => { },
                        () =>
                        {
                            ConsoleUtil.WriteLine("Port number must be a unsigned short integer.",
                                ConsoleColor.Red);
                        });
                }

                var capture = CaptureArgs.None;

                foreach (var arg in args)
                {
                    switch (capture)
                    {
                        case CaptureArgs.None:
                            if (arg.StartsWith("-", StringComparison.Ordinal) &&
                                !arg.StartsWith("--", StringComparison.Ordinal) && arg.Length > 1)
                            {
                                for (var i = 1; i < arg.Length; i++)
                                {
                                    switch (arg[i])
                                    {
                                        case 'c':
                                            NoSetCursor = true;
                                            break;

                                        case 'p':
                                            PrintControlMessages = true;
                                            break;

                                        case 'n':
                                            AutoFlush = false;
                                            break;

                                        case 'l':
                                            EnableLogging = false;
                                            break;

                                        case 'r':
                                            reconfigure = true;
                                            break;

                                        case 's':
                                            _stdPrint = true;
                                            break;

                                        case 'd':
                                            useDefault = true;
                                            break;

                                        case 'a':
                                            NoPadding = true;
                                            break;
                                    }
                                }
                            }
                            else
                                switch (arg)
                                {
                                    case "--noSetCursor":
                                        NoSetCursor = true;
                                        break;

                                    case "--printControl":
                                        PrintControlMessages = true;
                                        break;

                                    case "--noAutoFlush":
                                        AutoFlush = false;
                                        break;

                                    case "--noLogs":
                                        EnableLogging = false;
                                        break;

                                    case "--reconfigure":
                                        reconfigure = true;
                                        break;

                                    case "--printStd":
                                        _stdPrint = true;
                                        break;

                                    case "--useDefault":
                                        useDefault = true;
                                        break;

                                    case "--noAlign":
                                        NoPadding = true;
                                        break;

                                    case "--disableTrueColor":
                                        _noTrueColor = true;
                                        break;

                                    case "--dismissPluginManagerSecurityWarning":
                                        ConsoleUtil.WriteLine("Plugin manager has been enabled. USE AT YOUR OWN RISK!", ConsoleColor.Yellow);
                                        DismissPluginsSecurityWarning = true;
                                        break;

                                    case "--config":
                                        capture = CaptureArgs.ConfigPath;
                                        break;

                                    case "--logs":
                                        capture = CaptureArgs.LaLogsPath;
                                        break;

                                    case "--gameLogs":
                                        capture = CaptureArgs.GameLogsPath;
                                        break;

                                    case "--restartsLimit":
                                        capture = CaptureArgs.RestartsLimit;
                                        break;

                                    case "--restartsTimeWindow":
                                        capture = CaptureArgs.RestartsTimeWindow;
                                        break;

                                    case "--logLengthLimit":
                                        capture = CaptureArgs.LogLengthLimit;
                                        break;

                                    case "--logEntriesLimit":
                                        capture = CaptureArgs.LogEntriesLimit;
                                        break;

                                    case "--":
                                        capture = CaptureArgs.ArgsPassthrough;
                                        break;
                                }
                            break;

                        case CaptureArgs.ArgsPassthrough:
                            _gameArguments += $"\"{arg}\" ";
                            break;

                        case CaptureArgs.ConfigPath:
                            ConfigPath = arg;
                            capture = CaptureArgs.None;
                            break;

                        case CaptureArgs.LaLogsPath:
                            LaLogsPath = arg + Path.DirectorySeparatorChar;
                            capture = CaptureArgs.None;
                            break;

                        case CaptureArgs.GameLogsPath:
                            GameLogsPath = arg + Path.DirectorySeparatorChar;
                            capture = CaptureArgs.None;
                            break;

                        case CaptureArgs.RestartsLimit:
                            if (!int.TryParse(arg, out _restartsLimit) || _restartsLimit < -1)
                            {
                                _restartsLimit = 4;
                                ConsoleUtil.WriteLine("restartsLimit argument value must be an integer greater or equal to -1.", ConsoleColor.Red);
                            }
                            capture = CaptureArgs.None;
                            break;

                        case CaptureArgs.RestartsTimeWindow:
                            if (!int.TryParse(arg, out _restartsTimeWindow) || _restartsLimit < 0)
                            {
                                _restartsTimeWindow = 480;
                                ConsoleUtil.WriteLine("restartsTimeWindow argument value must be an integer greater or equal to 0.", ConsoleColor.Red);
                            }
                            capture = CaptureArgs.None;
                            break;

                        case CaptureArgs.LogLengthLimit:
                        {
                            string a = arg.Replace("k", "000", StringComparison.Ordinal)
                                .Replace("M", "000000", StringComparison.Ordinal)
                                .Replace("G", "000000000", StringComparison.Ordinal)
                                .Replace("T", "000000000000", StringComparison.Ordinal);
                            if (!ulong.TryParse(a, out LogLengthLimit))
                            {
                                ConsoleUtil.WriteLine(
                                    "logLengthLimit argument value must be an integer greater or equal to 0.",
                                    ConsoleColor.Red);
                                LogLengthLimit = 25000000000;
                            }

                            capture = CaptureArgs.None;
                        }
                            break;

                        case CaptureArgs.LogEntriesLimit:
                        {
                            string a = arg.Replace("k", "000", StringComparison.Ordinal)
                                .Replace("M", "000000", StringComparison.Ordinal)
                                .Replace("G", "000000000", StringComparison.Ordinal)
                                .Replace("T", "000000000000", StringComparison.Ordinal);
                            if (!ulong.TryParse(a, out LogEntriesLimit))
                            {
                                ConsoleUtil.WriteLine(
                                    "logEntriesLimit argument value must be an integer greater or equal to 0.",
                                    ConsoleColor.Red);
                                LogEntriesLimit = 10000000000;
                            }

                            capture = CaptureArgs.None;
                        }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (ConfigPath != null)
            {
                CurrentConfigPath = ConfigPath;

                if (File.Exists(ConfigPath))
                    Configuration = Config.DeserializeConfig(await File.ReadAllLinesAsync(ConfigPath, Encoding.UTF8));
                else reconfigure = true;
            }
            else
            {
                CurrentConfigPath = Path.Combine(PathManager.GameUserDataRoot, "config", GamePort.ToString(), "config_localadmin.txt");
                if (File.Exists(CurrentConfigPath))
                    Configuration = Config.DeserializeConfig(await File.ReadAllLinesAsync(CurrentConfigPath, Encoding.UTF8));
                else
                {
                    CurrentConfigPath = Path.Combine(PathManager.GameUserDataRoot, "config", GamePort.ToString(), "config_localadmin_global.txt");
                    if (File.Exists(CurrentConfigPath))
                        Configuration = Config.DeserializeConfig(await File.ReadAllLinesAsync(CurrentConfigPath, Encoding.UTF8));
                    else
                        reconfigure = true;
                }
            }

            if (reconfigure)
                ConfigWizard.RunConfigWizard(useDefault);

            NoSetCursor |= Configuration!.LaNoSetCursor;
            AutoFlush &= Configuration.LaLogAutoFlush;
            EnableLogging &= Configuration.EnableLaLogs;

            if (Configuration.EnableHeartbeat)
            {
                EnableGameHeartbeat = true;
                _heartbeatSpanMaxThreshold = Configuration.HeartbeatSpanMaxThreshold;
                _heartbeatRestartInSeconds = Configuration.HeartbeatRestartInSeconds;
                CurrentHeartbeatStatus = HeartbeatStatus.AwaitingFirstHeartbeat;
                HeartbeatStopwatch.Start();
                StartHeartbeatMonitoring();
            }

            InputQueue.Clear();

            if (_firstRun)
            {
                try
                {
                    SetupExitHandlers();
                }
                catch (Exception ex)
                {
                    ConsoleUtil.WriteLine(
                        $"Starting exit handlers threw {ex}. SCPSL Server will NOT be terminated when LocalAdmin closes!",
                        ConsoleColor.Yellow);
                }
            }

            if (_firstRun || _exit)
            {
                _exit = false;
                _firstRun = false;
                SetupKeyboardInput();
            }

            RegisterCommands();
            SetupReader();

            StartSession();

            _readerTask!.Start();

            if (!EnableLogging)
                ConsoleUtil.WriteLine("Logging has been disabled.", ConsoleColor.Red);
            else if (!AutoFlush)
                ConsoleUtil.WriteLine("Logs Auto-Flush has been disabled.", ConsoleColor.Yellow);

            if (PrintControlMessages)
                ConsoleUtil.WriteLine("Printing control messages been enabled using startup argument.", ConsoleColor.Gray);

            if (NoSetCursor)
                ConsoleUtil.WriteLine("Cursor management has been disabled.", ConsoleColor.Gray);

            if (Configuration.LaDeleteOldLogs || Configuration.DeleteOldRoundLogs || Configuration.CompressOldRoundLogs)
                LogCleaner.Initialize();

            while (!_exit)
                Thread.Sleep(250);

            // If the game was terminated intentionally, then wait, otherwise no
            Exit(0, true); // After the readerTask is completed this will happen
        }
        catch (Exception ex)
        {
            await File.WriteAllTextAsync($"LocalAdmin Crash {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ssZ}.txt", ex.ToString());

            Logger.Log("|===| Exception |===|");
            Logger.Log(ex);
            Logger.Log("|===================|");
            Logger.Log("");
        }
    }

    /// <summary>
    /// Starts a session,
    /// if the session has already begun,
    /// then terminates it.
    /// </summary>
    private void StartSession()
    {
        // Terminate the game, if the game process is exists
        if (_gameProcess is { HasExited: false })
            TerminateGame();

        Menu();

        BaseWindowTitle = $"LocalAdmin v. {VersionString} on port {GamePort}";
        Console.Title = BaseWindowTitle;

        Logger.Initialize();

        ConsoleUtil.WriteLine($"Started new session on port {GamePort}.", ConsoleColor.DarkGreen);
        ConsoleUtil.WriteLine("Trying to start server...", ConsoleColor.Gray);

        SetupServer();

        while (Server!.ConsolePort == 0)
            Thread.Sleep(200);

        RunScpsl();
    }

    private static void Menu()
    {
        ConsoleUtil.Clear();
        ConsoleUtil.WriteLine($"SCP: Secret Laboratory - LocalAdmin v. {VersionString}", ConsoleColor.Cyan);
        ConsoleUtil.WriteLine(string.Empty, ConsoleColor.Cyan);
        ConsoleUtil.WriteLine("Licensed under The MIT License (use command \"license\" to get license text).", ConsoleColor.Cyan);
        ConsoleUtil.WriteLine("Copyright by Łukasz \"zabszk\" Jurczyk and KernelError, 2019 - 2023", ConsoleColor.Cyan);
        ConsoleUtil.WriteLine(string.Empty, ConsoleColor.Cyan);
        ConsoleUtil.WriteLine("Type 'help' to get list of available commands.", ConsoleColor.Cyan);
        ConsoleUtil.WriteLine(string.Empty, ConsoleColor.Cyan);
    }

    private static void SetupExitHandlers()
    {
        ProcessHandler.Handler.Setup();
        AppDomainHandler.Handler.Setup();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowsHandler.Handler.Setup();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
#if LINUX_SIGNALS
                try
                {
                    UnixHandler.Handler.Setup();
                }
                catch (DllNotFoundException ex)
                {
                    if (!CheckMonoException(ex)) throw;
                }
                catch (EntryPointNotFoundException ex)
                {
                    if (!CheckMonoException(ex)) throw;
                }
                catch (TypeInitializationException ex)
                {
                    switch (ex.InnerException)
                    {
                        case DllNotFoundException dll:
                            if (!CheckMonoException(dll)) throw;
                            break;
                        case EntryPointNotFoundException dll:
                            if (!CheckMonoException(dll)) throw;
                            break;
                        default:
                            throw;
                    }
                }
#else
            ConsoleUtil.WriteLine("Invalid Linux build! Please download LocalAdmin from GitHub here: https://github.com/northwood-studios/LocalAdmin-V2/releases", ConsoleColor.Red);
#endif
        }
    }

#if LINUX_SIGNALS
        private static bool CheckMonoException(Exception ex)
        {
            if (!ex.Message.Contains("MonoPosixHelper")) return false;
            ConsoleUtil.WriteLine("Native exit handling for Linux requires Mono to be installed!", ConsoleColor.Yellow);
            return true;
        }
#endif

    private void SetupServer()
    {
        Server = new TcpServer();
        Server.Received += (_, line) =>
        {
            if (!byte.TryParse(line.AsSpan(0, 1), NumberStyles.HexNumber, null, out var colorValue))
                colorValue = (byte)ConsoleColor.Gray;

            ConsoleUtil.WriteLine(line[1..], (ConsoleColor)colorValue);
        };
        Server.Start();
    }

    private static void SetupKeyboardInput()
    {
        new Task(() =>
        {
            while (!_exit)
            {
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                InputQueue.Enqueue(input);
            }
        }).Start();
    }

    private void SetupReader()
    {
        async void ReaderTaskMethod()
        {
            while (Server == null) await Task.Delay(20);

            while (!_exit)
            {
                if (!InputQueue.TryDequeue(out var input))
                {
                    await Task.Delay(65);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var currentLineCursor = NoSetCursor ? 0 : Console.CursorTop;

                if (currentLineCursor > 0)
                {
                    Console.SetCursorPosition(0, currentLineCursor - 1);

                    ConsoleUtil.WriteLine($"{string.Empty.PadLeft(Console.WindowWidth)}>>> {input}", ConsoleColor.DarkMagenta, -1);
                    Console.SetCursorPosition(0, currentLineCursor);
                }
                else
                    ConsoleUtil.WriteLine($">>> {input}", ConsoleColor.DarkMagenta, -1);

                if (!_processRefreshFail && _gameProcess != null)
                {
                    try
                    {
                        if (_gameProcess.HasExited)
                        {
                            ConsoleUtil.WriteLine("Failed to send command - the game process was terminated...", ConsoleColor.Red);
                            _exit = true;
                            continue;
                        }
                    }
                    catch
                    {
                        _processRefreshFail = true;
                    }
                }

                var split = input.Split(' ');

                if (split.Length == 0) continue;
                var name = split[0].ToUpperInvariant();

                var command = _commandService.GetCommandByName(name);

                if (command != null)
                {
                    command.Execute(split.Skip(1).ToArray());
                    if (!command.SendToGame) continue;
                }

                var exit = false;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.StartsWith("exit ", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase) || input.StartsWith("quit ", StringComparison.OrdinalIgnoreCase) || input.Equals("stop", StringComparison.OrdinalIgnoreCase) || input.StartsWith("stop ", StringComparison.OrdinalIgnoreCase))
                {
                    DisableExitActionSignals = true;
                    ExitAction = ShutdownAction.SilentShutdown;
                    exit = true;
                }

                if (Server.Connected)
                    Server.WriteLine(input);
                else
                    ConsoleUtil.WriteLine("Failed to send command - connection to server process hasn't been established yet.", ConsoleColor.Yellow);

                if (!exit)
                    continue;

                await Task.Delay(100);
                _exit = true;
            }
        }

        _readerTask = new Task(ReaderTaskMethod);
    }

    private void RunScpsl()
    {
        if (File.Exists(_scpslExecutable))
        {
            ConsoleUtil.WriteLine("Executing: " + _scpslExecutable, ConsoleColor.DarkGreen);
            var printStd = Configuration!.LaShowStdoutStderr || _stdPrint;
            var redirectStreams =
                Configuration.LaLogStdoutStderr || printStd;

            var extraArgs = string.Empty;

            if (_noTrueColor || !Configuration.EnableTrueColor)
                extraArgs = " -disableAnsiColors";

            if (EnableGameHeartbeat)
                extraArgs += " -heartbeat";

            var startInfo = new ProcessStartInfo
            {
                FileName = _scpslExecutable,
                Arguments =
                    $"-batchmode -nographics -txbuffer {Configuration.SlToLaBufferSize} -rxbuffer {Configuration.LaToSlBufferSize} -port{GamePort} -console{Server!.ConsolePort} -id{Environment.ProcessId}{extraArgs} {_gameArguments}",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _gameProcess = Process.Start(startInfo);

            _gameProcess!.OutputDataReceived += (_, args) =>
            {
                if (!redirectStreams || string.IsNullOrWhiteSpace(args.Data))
                    return;

                ConsoleUtil.WriteLine("[STDOUT] " + args.Data, ConsoleColor.Gray,
                    log: Configuration.LaLogStdoutStderr,
                    display: printStd);
            };

            _gameProcess!.ErrorDataReceived += (_, args) =>
            {
                if (!redirectStreams || string.IsNullOrWhiteSpace(args.Data))
                    return;

                ConsoleUtil.WriteLine("[STDERR] " + args.Data, ConsoleColor.DarkMagenta,
                    log: Configuration.LaLogStdoutStderr,
                    display: printStd);
            };

            _gameProcess!.BeginOutputReadLine();
            _gameProcess!.BeginErrorReadLine();

            _gameProcess!.Exited += (_, _) =>
            {
                try
                {
                    if (_gameProcess is { HasExited: false })
                    {
                        ConsoleUtil.WriteLine("Game process exited and has been killed.", ConsoleColor.Gray);
                        _gameProcess.Kill();
                    }
                    else ConsoleUtil.WriteLine("Game process exited and has been killed, no need to kill it.", ConsoleColor.Gray);
                }
                catch (Exception e)
                {
                    ConsoleUtil.WriteLine($"Game process exited and has been killed, can't kill it: {e.Message}.", ConsoleColor.Gray);
                }

                if (_processClosing)
                    return;

                switch (ExitAction)
                {
                    case ShutdownAction.Crash:
                        ConsoleUtil.WriteLine("The game process has been terminated...", ConsoleColor.Red);
                        Exit(0, true);
                        break;

                    case ShutdownAction.Shutdown:
                        Exit(0, true);
                        break;

                    case ShutdownAction.SilentShutdown:
                        Exit(0);
                        break;

                    case ShutdownAction.Restart:
                        Exit(0, false, true);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            _gameProcess!.EnableRaisingEvents = true;
        }
        else
        {
            ConsoleUtil.WriteLine("Failed - Server application could not be located!", ConsoleColor.Red);

            DisableExitActionSignals = true;
            ExitAction = ShutdownAction.Shutdown;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Exit((int)WindowsErrorCode.ERROR_FILE_NOT_FOUND, true);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Exit((int)UnixErrorCode.ERROR_FILE_NOT_FOUND, true);
            else
                Exit(1);
        }
    }

    private void RegisterCommands()
    {
        _commandService.RegisterCommand(new HeartbeatCancelCommand());
        _commandService.RegisterCommand(new HeartbeatControlCommand());
        _commandService.RegisterCommand(new RestartCommand());
        _commandService.RegisterCommand(new ForceRestartCommand());
        _commandService.RegisterCommand(new HelpCommand());
        _commandService.RegisterCommand(new LicenseCommand());
        _commandService.RegisterCommand(new ResaveCommand());
        _commandService.RegisterCommand(new PluginManagerCommand());
        _commandService.RegisterCommand(new LaCfgCommand());
    }

    private static void ReadInput(Func<string?, bool> checkInput, Action validInputAction, Action invalidInputAction)
    {
        var input = Console.ReadLine();

        while (!checkInput(input))
        {
            invalidInputAction();

            input = Console.ReadLine();
        }

        validInputAction();
    }

    /// <summary>
    ///     Terminates the game.
    /// </summary>
    private void TerminateGame()
    {
        Server?.Stop();
        if (_gameProcess is { HasExited: false })
            _gameProcess.Kill();
    }

    /// <summary>
    ///     Terminates the game and console.
    /// </summary>
    public void Exit(int code = -1, bool waitForKey = false, bool restart = false)
    {
        lock (this)
        {
            if (_processClosing)
                return;

            _exit = true;
            _processClosing = true;
            LogCleaner.Abort();
            Logger.EndLogging();
            TerminateGame(); // Forcefully terminating the process
            _gameProcess?.Dispose();

            try
            {
                if (_readerTask is { IsCompleted: true })
                    _readerTask?.Dispose();
            }
            catch
            {
                //Ignore
            }

            try
            {
                if (_heartbeatMonitoringTask is { IsCompleted: true })
                    _heartbeatMonitoringTask?.Dispose();
            }
            catch
            {
                //Ignore
            }

            if (restart || ExitAction == ShutdownAction.Restart)
            {
                _ignoreNextRestart = true;
                return;
            }

            if (ExitAction == ShutdownAction.Crash && Configuration is { RestartOnCrash: true })
                return;

            if (waitForKey && ExitAction != ShutdownAction.SilentShutdown)
            {
                ConsoleUtil.WriteLine("Press any key to close...", ConsoleColor.DarkGray);
                Console.ReadKey(true);
            }

            Environment.Exit(code);
        }
    }

    /// <summary>
    ///     Releases resources used by the program
    /// </summary>
    public void Dispose()
    {
        Exit(0);
        GC.SuppressFinalize(this);
    }

    internal async Task LoadJsonOrTerminate()
    {
        try
        {
            if (!Directory.Exists(PathManager.ConfigPath))
                Directory.CreateDirectory(PathManager.ConfigPath);

            if (!File.Exists(PathManager.InternalJsonDataPath))
            {
                DataJson = new DataJson();
                await SaveJsonOrTerminate();
            }
            else
            {
                DataJson = await JsonFile.Load<DataJson>(PathManager.InternalJsonDataPath);
                JsonFile.UnlockFile(PathManager.InternalJsonDataPath);

                if (DataJson == null)
                {
                    ConsoleUtil.WriteLine("Json file is corrupted! Terminating LocalAdmin. If the issue persists, please delete the file and restart LocalAdmin.", ConsoleColor.Red);
                    Terminate();
                }
            }

            if (_previousPat != DataJson!.GitHubPersonalAccessToken)
            {
                _previousPat = DataJson.GitHubPersonalAccessToken;
                PluginInstaller.RefreshPat();
            }
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to read JSON config file: {e.Message}", ConsoleColor.Red);
            DataJson = null;
            Terminate();
            throw;
        }
    }

    internal async Task SaveJsonOrTerminate()
    {
        if (!(await DataJson!.TrySave(PathManager.InternalJsonDataPath)))
            Terminate();
    }

    private void Terminate()
    {
        DisableExitActionSignals = true;
        ExitAction = ShutdownAction.SilentShutdown;
        Exit(1);
    }

    internal static void HandleExitSignal()
    {
        Console.WriteLine("exit");
        InputQueue.Enqueue("exit");
    }

    internal void HandleHeartbeat()
    {
        switch (CurrentHeartbeatStatus)
        {
            case HeartbeatStatus.Disabled when EnableGameHeartbeat:
                return;

            case HeartbeatStatus.Disabled:
                ConsoleUtil.WriteLine("Received a heartbeat signal, but the heartbeat is disabled.", ConsoleColor.Yellow);
                return;

            case HeartbeatStatus.AwaitingFirstHeartbeat:
                ConsoleUtil.WriteLine("Received first heartbeat. Silent crash detection is now active.", ConsoleColor.DarkGreen);
                HeartbeatStopwatch.Restart();
                CurrentHeartbeatStatus = HeartbeatStatus.Active;
                break;

            default:
                HeartbeatStopwatch.Restart();
                break;
        }
    }

    private void StartHeartbeatMonitoring()
    {
        async void HeartbeatMonitoringMethod()
        {
            {
                ushort i = 0;

                while (CurrentHeartbeatStatus == HeartbeatStatus.AwaitingFirstHeartbeat)
                {
                    if (_exit)
                        return;

                    switch (i)
                    {
                        case 320: //80 seconds
                            ConsoleUtil.WriteLine(
                                "No heartbeat was received so far. Silent crash detection is NOT active.",
                                ConsoleColor.Yellow);
                            i++;
                            await Task.Delay(1000);
                            break;

                        case < 320:
                            i++;
                            await Task.Delay(250);
                            break;

                        default:
                            await Task.Delay(1000);
                            break;
                    }
                }
            }

            while (!_exit)
            {
                switch (CurrentHeartbeatStatus)
                {
                    case HeartbeatStatus.AwaitingFirstHeartbeat:
                    case HeartbeatStatus.Disabled:
                        await Task.Delay(1000);
                        continue;

                    case HeartbeatStatus.Active:
                        if (HeartbeatStopwatch.ElapsedMilliseconds <= (_heartbeatSpanMaxThreshold * 1000))
                        {
                            if (HeartbeatWarningStage != 0)
                                ConsoleUtil.WriteLine("Heartbeat has been received. Restart procedure aborted.", ConsoleColor.DarkGreen);

                            HeartbeatWarningStage = 0;
                            await Task.Delay(1800);
                            continue;
                        }

                        HeartbeatWarningStage++;

                        if (HeartbeatWarningStage >= _heartbeatRestartInSeconds)
                        {
                            ConsoleUtil.WriteLine("Game server has probably crashed. Restarting the server...", ConsoleColor.Red);
                            HeartbeatStopwatch.Reset();

                            DisableExitActionSignals = true;
                            ExitAction = ShutdownAction.Crash;

                            _exit = true;
                            return;
                        }

                        ConsoleUtil.WriteLine($"Game server has not sent a heartbeat in {HeartbeatStopwatch.ElapsedMilliseconds / 1000} seconds. Restarting the server in {_heartbeatRestartInSeconds - HeartbeatWarningStage} seconds! Type \"hbc\" command to abort!", ConsoleColor.Red);
                        await Task.Delay(1000);
                        break;
                }
            }
        }
        _heartbeatMonitoringTask = new Task(HeartbeatMonitoringMethod);
        _heartbeatMonitoringTask.Start();
    }

    ~LocalAdmin()
    {
        Exit(0);
    }
}