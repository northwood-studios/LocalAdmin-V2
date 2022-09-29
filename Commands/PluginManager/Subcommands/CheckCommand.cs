using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class CheckCommand
{
    internal static async void Check(string options)
    {
        bool iSet = options.Contains('i', StringComparison.OrdinalIgnoreCase),
            gSet = options.Contains('g', StringComparison.OrdinalIgnoreCase),
            lSet = options.Contains('l', StringComparison.OrdinalIgnoreCase);
        
        bool local = false, global = false;
        
        switch (gSet)
        {
            case false when !lSet:
            case true when lSet:
                local = global = true;
                break;
            
            case true:
                global = true;
                break;
            
            default:
                local = true;
                break;
        }

        if (local)
            await PluginUpdater.CheckForUpdates(Core.LocalAdmin.GamePort.ToString(), iSet);

        if (global)
            await PluginUpdater.CheckForUpdates("global", iSet);
        
        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Checking for plugins update complete.", ConsoleColor.DarkGreen);
    }
}