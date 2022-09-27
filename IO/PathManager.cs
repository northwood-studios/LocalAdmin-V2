using System;
using System.IO;

namespace LocalAdmin.V2.IO;

internal static class PathManager
{
    private static bool _configDirOverride;
    
    internal static readonly string GameUserDataRoot;
    internal static readonly string InternalJsonDataPath;
    
    static PathManager()
    {
        ProcessHostPolicy();

        GameUserDataRoot = _configDirOverride
            ? "AppData" + Path.DirectorySeparatorChar
            : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar +
              "SCP Secret Laboratory" + Path.DirectorySeparatorChar;

        InternalJsonDataPath =
            $"{GameUserDataRoot}config{Path.DirectorySeparatorChar}localadmin_internal_data.json";
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