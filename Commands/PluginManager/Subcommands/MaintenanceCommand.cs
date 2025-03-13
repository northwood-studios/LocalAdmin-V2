using System;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class MaintenanceCommand
{
    internal static async Task Maintenance(string options)
    {
        bool iSet = options.Contains('i', StringComparison.Ordinal),
            gSet = options.Contains('g', StringComparison.Ordinal),
            lSet = options.Contains('l', StringComparison.Ordinal);

        if (!lSet && !gSet)
        {
            lSet = gSet = true;
        }

        if (lSet)
            await PluginInstaller.PluginsMaintenance(Core.LocalAdmin.GamePort.ToString(), iSet);

        if (gSet)
            await PluginInstaller.PluginsMaintenance("global", iSet);

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugins maintenance complete.", ConsoleColor.DarkGreen);
    }
}