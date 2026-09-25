using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class UpdateCommand
{
    internal static async void Update(string options)
    {
        bool iSet = options.Contains('i', StringComparison.Ordinal),
            oSet = options.Contains('o', StringComparison.Ordinal),
            sSet = options.Contains('s', StringComparison.Ordinal);

        await PluginUpdater.UpdatePlugins(iSet, oSet, sSet);

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Updating plugins update complete.", ConsoleColor.DarkGreen);
    }
}