using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LocalAdmin.V2.IO;

internal static class PathManager
{
    private static bool _configDirOverride;

    internal static readonly string GameUserDataRoot;
    internal static readonly string ConfigPath;
    internal static readonly string InternalJsonDataPath;

    internal static bool CorrectPathFound { get; private set; }

    static PathManager()
    {
        ProcessHostPolicy();

        GameUserDataRoot = _configDirOverride
            ? "AppData" + Path.DirectorySeparatorChar
            : GetSpecialFolderPath() + "SCP Secret Laboratory" + Path.DirectorySeparatorChar;

        ConfigPath = $"{GameUserDataRoot}config{Path.DirectorySeparatorChar}";

        InternalJsonDataPath = ConfigPath + "localadmin_internal_data.json";
    }

    private static string GetSpecialFolderPath()
    {
        try
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!string.IsNullOrWhiteSpace(path))
            {
                CorrectPathFound = true;
                return path + Path.DirectorySeparatorChar;
            }

            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (!string.IsNullOrWhiteSpace(path))
            {
                CorrectPathFound = true;

                if (OperatingSystem.IsLinux())
                    return path + Path.DirectorySeparatorChar + ".config" + Path.DirectorySeparatorChar;

                if (OperatingSystem.IsWindows())
                    return path + Path.DirectorySeparatorChar + "AppData" + Path.DirectorySeparatorChar + "Roaming" + Path.DirectorySeparatorChar;

                ConsoleUtil.WriteLine("Failed to get special folder path - unsupported platform!", ConsoleColor.Red);
                throw new PlatformNotSupportedException();
            }

            CorrectPathFound = false;
            ConsoleUtil.WriteLine($"Failed to get special folder path - it's always null or empty!", ConsoleColor.Red);

            return string.Empty;
        }
        catch (Exception e)
        {
            CorrectPathFound = false;
            ConsoleUtil.WriteLine($"Failed to get special folder path! Exception: {e.Message}", ConsoleColor.Red);

            throw;
        }
    }

    private static void ProcessHostPolicy()
    {
        try
        {
            _configDirOverride = false;

            if (!File.Exists("hoster_policy.txt"))
                return;

            var lines = File.ReadAllLines("hoster_policy.txt");

            foreach (var l in lines)
            {
                if (!l.Contains("gamedir_for_configs: true", StringComparison.OrdinalIgnoreCase))
                    continue;

                _configDirOverride = true;
                ConsoleUtil.WriteLine("Applied policy: gamedir_for_configs: true", ConsoleColor.Gray);
                break;
            }
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to process hoster_policy.txt file: {e.Message}", ConsoleColor.Red);
        }
    }
}