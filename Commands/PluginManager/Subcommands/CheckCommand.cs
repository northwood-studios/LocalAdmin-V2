using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class CheckCommand
{
    internal static async void Check(string options)
    {
        await PluginUpdater.CheckForUpdates(options.Contains('i', StringComparison.Ordinal));

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Checking for plugins update complete.", ConsoleColor.DarkGreen);
    }
}