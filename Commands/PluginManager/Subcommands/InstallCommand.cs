using System;
using System.Linq;
using LocalAdmin.V2.Core;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class InstallCommand
{
    internal static async void Install(string[] args, string options)
    {
        PluginInstaller.QueryResult res;
        string version;
        
        var performUpdate = OfficialPluginsList.IsRefreshNeeded();

        if (options.Contains('s', StringComparison.Ordinal))
            performUpdate = false;
        
        if (performUpdate)
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Refreshing plugins list...", ConsoleColor.Yellow);
            await OfficialPluginsList.RefreshOfficialPluginsList();
        }

        args[0] = OfficialPluginsList.ResolvePluginAlias(args[0], PluginAliasFlags.CanInstall);

        if (args[0].Count(x => x == '/') != 1)
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugin name is invalid!", ConsoleColor.Red);
            return;
        }

        if (args.Length == 1 || args.Length == 2 && args[1].Equals("latest", StringComparison.OrdinalIgnoreCase))
        {
            if (!performUpdate)
                await Core.LocalAdmin.Singleton!.LoadJsonOrTerminate();
            
            res = await PluginInstaller.TryCachePlugin(args[0], true);
            if (!res.Success)
                return;

            if (!await Core.LocalAdmin.DataJson!.TrySave(PathManager.InternalJsonDataPath))
                return;
            
            version = "latest";
        }
        else
        {
            if (args[1].Contains('/', StringComparison.Ordinal))
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Invalid plugin version!", ConsoleColor.Red);
                return;
            }
            
            res = await PluginInstaller.TryGetVersionDetails(args[0], args[1]);
            if (!res.Success)
                return;

            version = args[1];
        }

        await PluginInstaller.TryInstallPlugin(args[0], res.Result, version,
            options.Contains('g', StringComparison.Ordinal) ? "global" : Core.LocalAdmin.GamePort.ToString(),
            options.Contains('o', StringComparison.Ordinal),
            options.Contains('i', StringComparison.Ordinal));
    }
}