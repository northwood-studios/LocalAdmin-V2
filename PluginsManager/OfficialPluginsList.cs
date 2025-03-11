using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.JSON;
using LocalAdmin.V2.JSON.Objects;

namespace LocalAdmin.V2.PluginsManager;

internal static class OfficialPluginsList
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(45),
        DefaultRequestHeaders = { { "User-Agent", $"LocalAdmin v. {Core.LocalAdmin.VersionString}" } }
    };

    internal static bool IsRefreshNeeded()
    {
        if (Core.LocalAdmin.DataJson!.LastPluginAliasesRefresh == null)
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugins list refresh was never performed.", ConsoleColor.Yellow);
            return true;
        }

        if ((DateTime.UtcNow - Core.LocalAdmin.DataJson.LastPluginAliasesRefresh).Value.TotalMinutes <= 30)
            return false;

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Last plugins list refresh was was performed more than 30 minutes ago.",
            ConsoleColor.Yellow);
        return true;
    }

    internal static async Task RefreshOfficialPluginsList()
    {
        try
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Refreshing plugins list...", ConsoleColor.Blue);
            var response = await HttpClient.GetAsync("https://gra2.scpslgame.com/localadmin.php?v=1");

            if (!response.IsSuccessStatusCode)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Refreshing failed with code {response.StatusCode}. Retrying with second server...", ConsoleColor.Yellow);
                response = await HttpClient.GetAsync("https://api.scpslgame.com/localadmin.php?v=1");
            }

            if (!response.IsSuccessStatusCode)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to refresh plugins list! (Status code: {response.StatusCode})", ConsoleColor.Red);
                return;
            }

            var data = JsonSerializer.Deserialize(await response.Content.ReadAsStreamAsync(), JsonGenerated.Default.DictionaryStringPluginAlias);

            if (data == null)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to refresh plugins list! (deserialization error)", ConsoleColor.Red);
                return;
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading LocalAdmin config file...", ConsoleColor.Blue);
            await Core.LocalAdmin.Singleton.LoadJsonOrTerminate();

            Core.LocalAdmin.DataJson = Core.LocalAdmin.DataJson! with { PluginAliases = data, LastPluginAliasesRefresh = DateTime.UtcNow };

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing LocalAdmin config file...", ConsoleColor.Blue);
            await Core.LocalAdmin.Singleton.SaveJsonOrTerminate();

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugins list has been refreshed!", ConsoleColor.DarkGreen);
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to refresh plugins list! Exception: {e.Message}", ConsoleColor.Red);
        }
    }

    internal static string ResolvePluginAlias(string alias, PluginAliasFlags requiredFlags)
    {
        if (Core.LocalAdmin.DataJson == null || Core.LocalAdmin.DataJson.PluginAliases == null ||
            !Core.LocalAdmin.DataJson.PluginAliases.TryGetValue(alias, out PluginAlias pluginAlias))
            return alias;
        if (((PluginAliasFlags)pluginAlias.Flags & requiredFlags) == 0)
            return alias;

        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin name {alias} has been resolved to {pluginAlias.Repository}!", ConsoleColor.Gray);

        return pluginAlias.Repository;
    }
}