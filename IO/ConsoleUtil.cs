using System;
using System.Globalization;
using System.IO;
using LocalAdmin.V2.IO.Logging;

namespace LocalAdmin.V2.IO;

public static class ConsoleUtil
{
    private static readonly char[] ToTrim = { '\n', '\r' };

    private static readonly object Lck = new object();

    private static string? _liveTimestampPadding, _logsTimestampPadding;

    public static void Clear()
    {
        lock (Lck)
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                //Ignore
            }
        }
    }

    private static string GetLogsLocalTimestamp() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}]";

    private static string GetLogsUtcTimestamp() => Core.LocalAdmin.Configuration is { LaLogsUseZForUtc: true }
        ? $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}Z]"
        : $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}]";

    public static string GetLogsTimestamp() => Core.LocalAdmin.Configuration is { LaLogsUseUtc: true }
        ? GetLogsUtcTimestamp()
        : GetLogsLocalTimestamp();

    private static string GetLiveViewLocalTimestamp() => $"[{DateTime.Now.ToString(Core.LocalAdmin.Configuration!.LaLiveViewTimeFormat, CultureInfo.InvariantCulture)}]";

    private static string GetLiveViewUtcTimestamp() => $"[{DateTime.UtcNow.ToString(Core.LocalAdmin.Configuration!.LaLiveViewTimeFormat, CultureInfo.InvariantCulture)}]";

    private static string GetLiveViewTimestamp() => Core.LocalAdmin.Configuration == null ? GetLogsLocalTimestamp() : Core.LocalAdmin.Configuration.LaLiveViewUseUtc
        ? GetLiveViewUtcTimestamp()
        : GetLiveViewLocalTimestamp();

    private static string GetLiveViewPadding()
    {
        if (_liveTimestampPadding == null)
        {
            int l = GetLiveViewTimestamp().Length + 1;
            _liveTimestampPadding = "\n";
            for (int i = 0; i < l; i++)
                _liveTimestampPadding += " ";
        }

        return _liveTimestampPadding;
    }

    private static string GetLogsPadding()
    {
        if (_logsTimestampPadding == null)
        {
            int l = GetLogsTimestamp().Length + 1;
            _logsTimestampPadding = "\n";
            for (int i = 0; i < l; i++)
                _logsTimestampPadding += " ";
        }

        return _logsTimestampPadding;
    }

    public static void Write(string content, ConsoleColor color = ConsoleColor.White, int height = 0, bool log = true, bool display = true)
    {
        lock (Lck)
        {
            content = string.IsNullOrEmpty(content) ? string.Empty : content.TrimStart().Trim(ToTrim);
            bool multiline = !Core.LocalAdmin.NoPadding && content.Contains('\n', StringComparison.Ordinal);

            if (display)
            {
                try
                {
                    Console.ForegroundColor = color;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (height > 0 && !Core.LocalAdmin.NoSetCursor)
                    Console.CursorTop += height;

                Console.Write($"{GetLiveViewTimestamp()} {(multiline ? content.Replace("\n", GetLiveViewPadding(), StringComparison.Ordinal) : content)}");

                Console.ForegroundColor = ConsoleColor.White;
            }

            if (log)
                Logger.Log($"{GetLogsTimestamp()} {(multiline ? content.Replace("\n", GetLogsPadding(), StringComparison.Ordinal) : content)}");
        }
    }

    public static void WriteLine(string? content, ConsoleColor color = ConsoleColor.White, int height = 0, bool log = true, bool display = true)
    {
        lock (Lck)
        {
            content = string.IsNullOrEmpty(content) ? string.Empty : content.Trim().Trim(ToTrim);
            bool multiline = !Core.LocalAdmin.NoPadding && content.Contains('\n', StringComparison.Ordinal);

            if (display)
            {
                try
                {
                    Console.ForegroundColor = color;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (height > 0 && !Core.LocalAdmin.NoSetCursor)
                    Console.CursorTop += height;

                Console.WriteLine($"{GetLiveViewTimestamp()} {(multiline ? content.Replace("\n", GetLiveViewPadding(), StringComparison.Ordinal) : content)}");

                Console.ForegroundColor = ConsoleColor.White;
            }

            if (log)
                Logger.Log($"{GetLogsTimestamp()} {(multiline ? content.Replace("\n", GetLogsPadding(), StringComparison.Ordinal) : content)}");
        }
    }
}