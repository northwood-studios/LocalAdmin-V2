using LocalAdmin.V2.IO;
using System;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LocalAdmin.V2.PluginsManager;

internal static class PluginPaths
{
    internal const string PortFolder = "$port";

    internal static string PluginsFolder { get; private set; } = PortFolder;

    internal static string DependenciesFolder { get; private set; } = PortFolder;

    private static LabApiConfig _labApiConfig = new();

    private static bool _initialized = false;

    internal static string FormatPath(string path) => path.Replace(PortFolder, Core.LocalAdmin.GamePort.ToString());

    internal static void LoadLabApiConfig()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            var configPath = $"{PathManager.LabApiRoot}LabApi-{Core.LocalAdmin.GamePort}.yml";

            if (!File.Exists(configPath))
            {
                ConsoleUtil.WriteLine("LabAPI configuration file is missing, using defaults for plugins management", ConsoleColor.Yellow);
                return;
            }

            var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
            var newConfig = deserializer.Deserialize<LabApiConfig>(File.ReadAllText(configPath));

            if (newConfig is null)
            {
                ConsoleUtil.WriteLine("Loaded LabAPI configuration is null, using defaults for plugins management", ConsoleColor.Yellow);
                return;
            }

            _labApiConfig = newConfig;

            if (_labApiConfig.PluginPaths.Count > 0)
            {
                PluginsFolder = _labApiConfig.PluginPaths.Last();
            }
            else
            {
                PluginsFolder = PortFolder;
                ConsoleUtil.WriteLine("No plugin paths configured, using default port folder instead", ConsoleColor.Yellow);
            }

            if (_labApiConfig.DependencyPaths.Count > 0)
            {
                DependenciesFolder = _labApiConfig.DependencyPaths.Last();
            }
            else
            {
                DependenciesFolder = PortFolder;
                ConsoleUtil.WriteLine("No dependency paths configured, using default port folder instead", ConsoleColor.Yellow);
            }
        }
        catch (Exception ex)
        {
            ConsoleUtil.WriteLine($"LabAPI config load failed. Exception: {ex.InnerException?.Message ?? ex.Message}", ConsoleColor.Yellow);
            ConsoleUtil.WriteLine("Failed to load LabAPI configuration, using defaults for plugins management", ConsoleColor.Yellow);
        }
    }

    internal static bool SetPluginsFolder(string path)
    {
        if (!_labApiConfig.PluginPaths.Contains(path))
        {
            return false;
        }

        PluginsFolder = path;
        return true;
    }

    internal static bool SetDependenciesFolder(string path)
    {
        if (!_labApiConfig.DependencyPaths.Contains(path))
        {
            return false;
        }

        DependenciesFolder = path;
        return true;
    }
}