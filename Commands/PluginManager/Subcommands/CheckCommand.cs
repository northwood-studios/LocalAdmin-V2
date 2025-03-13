using System;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class CheckCommand
{
    internal static async Task Check(string options)
    {
        bool iSet = options.Contains('i', StringComparison.Ordinal),
            gSet = options.Contains('g', StringComparison.Ordinal),
            lSet = options.Contains('l', StringComparison.Ordinal);

        if (!lSet && !gSet)
        {
            lSet = gSet = true;
        }

        if (lSet)
            await PluginUpdater.CheckForUpdates(Core.LocalAdmin.GamePort.ToString(), iSet);

        if (gSet)
            await PluginUpdater.CheckForUpdates("global", iSet);

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Checking for plugins update complete.", ConsoleColor.DarkGreen);
    }
}