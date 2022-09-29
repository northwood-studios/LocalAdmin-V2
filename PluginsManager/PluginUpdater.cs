using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LocalAdmin.V2.Core;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.PluginsManager;

internal static class PluginUpdater
{
    internal static async Task<bool> CheckForUpdates(string port, bool ignoreLocks)
    {
        var metadataPath = PluginInstaller.PluginsPath(port);
        
        try
        {
            if (!Directory.Exists(metadataPath))
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] No metadata file for port {port}. Skipped.", ConsoleColor.Blue);
                return true;
            }
            
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Reading metadata for port {port}...", ConsoleColor.Blue);
            var metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : PluginInstaller.DefaultLockTime, true);

            if (metadata == null || metadata.InstalledPlugins.Count == 0)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] No plugins installed for port {port}. Skipped.", ConsoleColor.Blue);
                JsonFile.UnlockFile(metadataPath);
                return true;
            }
            
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading LocalAdmin config file...", ConsoleColor.Blue);
            await Core.LocalAdmin.Singleton!.LoadJsonOrTerminate();

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Processing installed plugins...", ConsoleColor.Blue);

            int ok = 0, failed = 0, outdated = 0, fixedOutdated = 0, i = 0;
            
            foreach (var plugin in metadata.InstalledPlugins)
            {
                i++;
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Querying plugin {plugin.Key} ({i}/{metadata.InstalledPlugins.Count})...", ConsoleColor.Blue);
                var qr = await PluginInstaller.TryCachePlugin(plugin.Key, true);

                if (!qr.Success)
                {
                    failed++;
                    continue;
                }

                if (qr.Result.Version == plugin.Value.CurrentVersion)
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} (v. {plugin.Value.CurrentVersion}) is up to date!", ConsoleColor.DarkGreen);
                    ok++;
                    continue;
                }

                if (plugin.Value.TargetVersion == null ||
                    plugin.Value.TargetVersion.Equals("latest", StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} (v. {plugin.Value.CurrentVersion}) is outdated! Latest version: {qr.Result.Version}.", ConsoleColor.Yellow);
                    outdated++;
                    continue;
                }

                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} (v. {plugin.Value.CurrentVersion}) is outdated, but a specific version was installed! Latest version: {qr.Result.Version}.", ConsoleColor.Gray);
                fixedOutdated++;
            }
            
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Finished checking for plugins updates for port {port}. Up to date: {ok}, outdated: {outdated}, outdated (with specific version set): {fixedOutdated}, failed: {failed}.", ConsoleColor.DarkGreen);
            
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing LocalAdmin config file...", ConsoleColor.Blue);
            await Core.LocalAdmin.Singleton.SaveJsonOrTerminate();
            
            metadata.LastUpdateCheck = DateTime.UtcNow;
            
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);
            if (await metadata.TrySave(metadataPath, 0, true))
                return true;
            
            ConsoleUtil.WriteLine(
                "[PLUGIN MANAGER] Failed to save metadata.",
                ConsoleColor.Red);

            return false;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to check for plugin updates for port {port}! Exception: {e.Message}",
                ConsoleColor.Red);
            JsonFile.UnlockFile(metadataPath);
            
            return false;
        }
    }

    internal static async Task UpdatePlugins(string port, bool ignoreLocks, bool overwrite)
    {
        var metadataPath = PluginInstaller.PluginsPath(port);
        var pluginsPath = PluginInstaller.PluginsPath(port);

        try
        {
            if (!Directory.Exists(metadataPath) || !Directory.Exists(pluginsPath))
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] No metadata file for port {port}. Skipped.", ConsoleColor.Blue);
                return;
            }
            
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Reading metadata for port {port}...", ConsoleColor.Blue);
            var metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : PluginInstaller.DefaultLockTime);

            if (metadata == null || metadata.InstalledPlugins.Count == 0)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] No plugins installed for port {port}. Skipped.", ConsoleColor.Blue);
                return;
            }

            var performUpdate = false;
            
            if (metadata.LastUpdateCheck == null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Last plugins update check for port {port} was never performed.", ConsoleColor.Yellow);
                performUpdate = true;
            }
            else if ((DateTime.UtcNow - metadata.LastUpdateCheck).Value.TotalMinutes > 30)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Last plugins update check for port {port} was performed more than 30 minutes ago.", ConsoleColor.Yellow);
                performUpdate = true;
            }

            if (performUpdate)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Performing plugins update check...", ConsoleColor.Yellow);

                if (!await CheckForUpdates(port, ignoreLocks))
                {
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugins update check failed! Aborting plugins update.", ConsoleColor.Red);
                    return;
                }
                
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Reading metadata for port {port}...", ConsoleColor.Blue);
                metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : PluginInstaller.DefaultLockTime);
                
                if (metadata == null || metadata.InstalledPlugins.Count == 0)
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] No plugins installed for port {port}. Skipped.", ConsoleColor.Blue);
                    return;
                }
            }
            
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading LocalAdmin config file...", ConsoleColor.Blue);
            await Core.LocalAdmin.Singleton!.LoadJsonOrTerminate();

            var i = 0;
            List<string> toRemove = new();

            foreach (var plugin in metadata.InstalledPlugins)
            {
                i++;
                var safeName = plugin.Key.Replace("/", "_", StringComparison.Ordinal);
                var pluginPath = pluginsPath + $"{safeName}.dll";
                
                if (!File.Exists(pluginPath))
                {
                    toRemove.Add(plugin.Key);
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} has been manually uninstalled. Skipped.", ConsoleColor.Gray);
                    continue;
                }
                
                var currentHash = Sha.Sha256File(pluginPath);

                if (currentHash != plugin.Value.FileHash)
                {
                    if (!overwrite)
                    {
                        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} has been manually updated! Run update with \"-o\" argument to overwrite. Skipped.", ConsoleColor.Gray);
                        continue;
                    }
                    
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} has been manually updated!", ConsoleColor.Gray);
                }
                
                if (plugin.Value.TargetVersion != null &&
                    !plugin.Value.TargetVersion.Equals("latest", StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} has a specific version set. Skipped.", ConsoleColor.Gray);
                    continue;
                }
                
                if (!Core.LocalAdmin.DataJson!.PluginVersionCache!.ContainsKey(plugin.Key))
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} is not cached! Skipped.", ConsoleColor.Yellow);
                    continue;
                }
                
                var cachedPlugin = Core.LocalAdmin.DataJson.PluginVersionCache[plugin.Key];
                
                if (cachedPlugin.Version.Equals(plugin.Value.CurrentVersion, StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} is up to date! Skipped.", ConsoleColor.DarkGreen);
                    continue;
                }

                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Updating plugin {plugin.Key} ({i}/{metadata.InstalledPlugins.Count})...", ConsoleColor.Blue);
                await PluginInstaller.TryInstallPlugin(plugin.Key, cachedPlugin, "latest", port, overwrite, ignoreLocks);
            }

            if (toRemove.Count != 0)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Removing manually uninstalled plugins from metadata file...", ConsoleColor.Blue);
                
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Reading metadata for port {port}...", ConsoleColor.Blue);
                metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : PluginInstaller.DefaultLockTime, true);
                
                if (metadata == null || metadata.InstalledPlugins.Count == 0)
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Reading metadata filed.", ConsoleColor.Red);
                    JsonFile.UnlockFile(metadataPath);
                    return;
                }
                
                foreach (var plugin in toRemove)
                {
                    metadata.InstalledPlugins.Remove(plugin);
                }
                
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);
                if (!await metadata.TrySave(metadataPath, 0, true))
                    return;
                
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata complete.", ConsoleColor.Blue);
            }
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to update plugins for port {port}! Exception: {e.Message}",
                ConsoleColor.Red);
            JsonFile.UnlockFile(metadataPath);
        }
    }
}