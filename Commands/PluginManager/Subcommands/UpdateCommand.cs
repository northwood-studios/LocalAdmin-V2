using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class UpdateCommand
{
    internal static async void Update(string options)
    {
        bool iSet = options.Contains('i', StringComparison.OrdinalIgnoreCase),
            gSet = options.Contains('g', StringComparison.OrdinalIgnoreCase),
            lSet = options.Contains('l', StringComparison.OrdinalIgnoreCase),
            oSet = options.Contains('l', StringComparison.OrdinalIgnoreCase);

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
            await PluginUpdater.UpdatePlugins(Core.LocalAdmin.GamePort.ToString(), iSet, oSet);

        if (global)
            await PluginUpdater.UpdatePlugins("global", iSet, oSet);
        
        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Updating plugins update complete.", ConsoleColor.DarkGreen);
    }
}