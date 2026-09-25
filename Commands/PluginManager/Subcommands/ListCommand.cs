using System;
using System.Collections.Generic;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class ListCommand
{
    internal static async void List(string options)
    {
        bool iSet = options.Contains('i', StringComparison.Ordinal),
            sSet = options.Contains('s', StringComparison.Ordinal);

        List<PluginStorage.PluginListEntry>? plugins = await PluginStorage.ListPlugins(iSet, sSet);
        ConsoleUtil.WriteLine(null, ConsoleColor.Gray);
        PrintPlugins(Core.LocalAdmin.GamePort.ToString(), plugins);
    }

    private static void PrintPlugins(string port, List<PluginStorage.PluginListEntry>? plugins)
    {
        ConsoleUtil.WriteLine($"------ PLUGINS FOR PORT {port.ToUpperInvariant()} ------", ConsoleColor.DarkGreen);

        if (plugins == null || plugins.Count == 0)
            ConsoleUtil.WriteLine("No plugins installed.", ConsoleColor.Gray);
        else
        {
            var i = 0;

            foreach (var plugin in plugins)
            {
                ConsoleColor color;
                var upToDate = plugin.UpToDate;

                if (!plugin.IntegrityCheckPassed)
                    color = ConsoleColor.Red;
                else if (upToDate)
                    color = ConsoleColor.Gray;
                else color = ConsoleColor.Yellow;

                if (i != 0)
                    ConsoleUtil.WriteLine(null, ConsoleColor.Gray);

                ConsoleUtil.WriteLine($"{++i}. {plugin.Name} ({plugin.InstalledVersionValidated})", color);
                ConsoleUtil.WriteLine($"Latest version: {plugin.LatestVersion}" + (upToDate ? "" : " >>> UPDATE AVAILABLE <<<"), color);
                ConsoleUtil.WriteLine($"Target version: {plugin.TargetVersion}", color);
                ConsoleUtil.WriteLine($"Plugin integrity check: {(plugin.IntegrityCheckPassed ? "PASSED" : "FAILED - PLUGIN MANUALLY MODIFIED")}", color);
                ConsoleUtil.WriteLine($"Dependencies: {(plugin.Dependencies == null ? "(none)" : string.Join(", ", plugin.Dependencies))}", color);
            }
        }

        ConsoleUtil.WriteLine("------------", ConsoleColor.DarkGreen);
        ConsoleUtil.WriteLine(null, ConsoleColor.Gray);
    }
}