using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class MaintenanceCommand
{
    internal static async void Maintenance(string options)
    {
        await PluginInstaller.PluginsMaintenance(options.Contains('i', StringComparison.Ordinal));

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugins maintenance complete.", ConsoleColor.DarkGreen);
    }
}