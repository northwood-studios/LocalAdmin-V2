using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace LocalAdmin.V2.IO;

using LocalAdmin = Core.LocalAdmin;

internal static class CommandsInput
{
    internal const string IntroText = "> ";
    internal const int IntroLength = 2;

    private const int HistorySize = 15;

    public static IReadOnlyList<string> History => _commandsHistory;

    public static string CurrentInput
    {
        get
        {
            lock (_lock)
            {
                return _currentInput;
            }
        }
        private set
        {
            _currentInput = value;
        }
    }

    private static readonly ConcurrentQueue<string> _commandsQueue = new();
    private static readonly List<string> _commandsHistory = new();
    private static readonly object _lock = new();

    private static bool _useNewMethod = false;
    private static string _currentInput = "";

    public static void Start(bool useNewMethod)
    {
        _useNewMethod = useNewMethod;
        Task.Run(HandleInputInternal);
    }

    public static bool TryDequeueCommand([MaybeNullWhen(false)] out string result)
    {
        return _commandsQueue.TryDequeue(out result);
    }

    internal static void EnqueueCommand(string command)
    {
        _commandsQueue.Enqueue(command);
    }

    internal static void ClearQueue()
    {
        _commandsQueue.Clear();
    }

    internal static void AddToHistory(string command)
    {
        if (!_commandsHistory.Remove(command) && _commandsHistory.Count == HistorySize)
            _commandsHistory.RemoveAt(_commandsHistory.Count-1);

        _commandsHistory.Insert(0, command);
    }

    private static void HandleInputInternal()
    {
        ConsoleKeyInfo consoleKeyInfo;
        int historyIndex = -1;

        void ReadByNewMethod()
        {
            MethodEntry:
            ConsoleUtil.ResetCurrentLine();
            ConsoleUtil.WriteCommandsInput();

            do
            {
#if LINUX_SIGNALS
                while (!Console.KeyAvailable && !LocalAdmin.Exited)
                {
                    Task.Delay(50).Wait();
                }

                if (LocalAdmin.Exited)
                    return;
#endif

                consoleKeyInfo = Console.ReadKey(intercept: true);

                if (consoleKeyInfo.Key == ConsoleKey.UpArrow && _commandsHistory.Count > 0)
                {
                    historyIndex = Math.Abs((historyIndex + 1) % _commandsHistory.Count);
                    CurrentInput = _commandsHistory[historyIndex];
                    goto MethodEntry;
                }
                else if (consoleKeyInfo.Key == ConsoleKey.DownArrow && _commandsHistory.Count > 0)
                {
                    historyIndex = Math.Abs((historyIndex - 1) % _commandsHistory.Count);
                    CurrentInput = _commandsHistory[historyIndex];
                    goto MethodEntry;
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Backspace && CurrentInput.Length > 0)
                {
                    CurrentInput = CurrentInput[..^1];
                    ConsoleUtil.RemoveLastCharacter();
                }
                else if (!char.IsControl(consoleKeyInfo.KeyChar) && IntroLength+CurrentInput.Length < Console.BufferWidth)
                {
                    CurrentInput += consoleKeyInfo.KeyChar;
                    ConsoleUtil.WriteChar(consoleKeyInfo.KeyChar);
                }
            } while (consoleKeyInfo.Key != ConsoleKey.Enter && !LocalAdmin.Exited);

            if (LocalAdmin.NoSetCursor)
                Console.WriteLine();
        }

        while (!LocalAdmin.Exited)
        {
            if (_useNewMethod)
                ReadByNewMethod();
            else
                CurrentInput = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrEmpty(CurrentInput))
                continue;

            AddToHistory(CurrentInput);

            _commandsQueue.Enqueue(CurrentInput);
            CurrentInput = "";
        }
    }
}