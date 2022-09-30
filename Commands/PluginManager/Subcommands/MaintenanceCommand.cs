using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class MaintenanceCommand
{
    internal static async void Maintenance(string options)
    {
        bool iSet = options.Contains('i', StringComparison.Ordinal),
            gSet = options.Contains('g', StringComparison.Ordinal),
            lSet = options.Contains('l', StringComparison.Ordinal);

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
            await PluginInstaller.DependenciesMaintenance(Core.LocalAdmin.GamePort.ToString(), iSet);

        if (global)
            await PluginInstaller.DependenciesMaintenance("global", iSet);
        
        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugins maintenance complete.", ConsoleColor.DarkGreen);
    }
}