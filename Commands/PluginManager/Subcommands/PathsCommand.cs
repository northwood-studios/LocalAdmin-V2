using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class PathsCommand
{
    internal static void ManagePaths(string[]? args, string options)
    {
        if (args is not null && args.Length > 0 && !string.IsNullOrEmpty(args[0]))
        {
            if (options.Contains('p', StringComparison.Ordinal))
            {
                var success = PluginContext.SetPluginsFolder(args[0]);

                if (!success)
                {
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to change plugins path. Check your LabAPI configuration.", ConsoleColor.Yellow);
                }
            }

            if (options.Contains('d', StringComparison.Ordinal))
            {
                var success = PluginContext.SetDependenciesFolder(args[0]);

                if (!success)
                {
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to change dependencies path. Check your LabAPI configuration.", ConsoleColor.Yellow);
                }
            }
        }

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Current paths configuration used for plugins management:", ConsoleColor.Blue);
        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugins path: {PluginContext.PluginsFolder}", ConsoleColor.Blue);
        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Dependencies path: {PluginContext.DependenciesFolder}", ConsoleColor.Blue);
    }
}