using System;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class UpdateCommand
{
    internal static async Task Update(string options)
    {
        bool iSet = options.Contains('i', StringComparison.Ordinal),
            gSet = options.Contains('g', StringComparison.Ordinal),
            lSet = options.Contains('l', StringComparison.Ordinal),
            oSet = options.Contains('o', StringComparison.Ordinal),
            sSet = options.Contains('s', StringComparison.Ordinal);

        if (!lSet && !gSet)
        {
            lSet = gSet = true;
        }

        if (lSet)
            await PluginUpdater.UpdatePlugins(Core.LocalAdmin.GamePort.ToString(), iSet, oSet, sSet);

        if (gSet)
            await PluginUpdater.UpdatePlugins("global", iSet, oSet, sSet);

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Updating plugins update complete.", ConsoleColor.DarkGreen);
    }
}