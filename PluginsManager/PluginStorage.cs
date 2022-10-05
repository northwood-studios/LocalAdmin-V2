using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.PluginsManager;

internal static class PluginStorage
{
    internal static async Task<List<PluginListEntry>?> ListPlugins(string port, bool ignoreLocks, bool skipUpdateCheck)
    {
        try
        {
            var pluginsPath = PluginInstaller.PluginsPath(port);

            if (!Directory.Exists(pluginsPath))
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugins path for port {port} doesn't exist. Skipped",
                    ConsoleColor.Blue);
                return null;
            }

            var metadataPath = pluginsPath + "metadata.json";
            
            if (!File.Exists(metadataPath))
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Metadata file for port {port} doesn't exist. Skipped.", ConsoleColor.Blue);
                return null;
            }
            
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading metadata...", ConsoleColor.Blue);
            var metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : PluginInstaller.DefaultLockTime);
            
            if (metadata == null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to parse metadata file for port {port}!", ConsoleColor.Red);
                return null;
            }
            
            var performUpdate = false;
            
            if (metadata.LastUpdateCheck == null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugins update check for port {port} was never performed.", ConsoleColor.Yellow);
                performUpdate = true;
            }
            else if ((DateTime.UtcNow - metadata.LastUpdateCheck).Value.TotalMinutes > 30)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Last plugins update check for port {port} was performed more than 30 minutes ago.", ConsoleColor.Yellow);
                performUpdate = true;
            }

            if (performUpdate && !skipUpdateCheck)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Performing plugins update check...", ConsoleColor.Yellow);

                if (!await PluginUpdater.CheckForUpdates(port, ignoreLocks))
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugins update check failed! Aborting plugins update.", ConsoleColor.Yellow);

                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Reading metadata for port {port}...", ConsoleColor.Blue);
                metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : PluginInstaller.DefaultLockTime);
                
                if (metadata == null || metadata.InstalledPlugins.Count == 0)
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] No plugins installed for port {port}. Skipped.", ConsoleColor.Blue);
                    return null;
                }
            }
            
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading LocalAdmin config file...", ConsoleColor.Blue);
            await Core.LocalAdmin.Singleton!.LoadJsonOrTerminate();
            
            List<PluginListEntry> plugins = new();
            
            foreach (var plugin in metadata.InstalledPlugins)
            {
                var pluginPath = pluginsPath + $"{plugin.Key.Replace("/", "_", StringComparison.Ordinal)}.dll";
                
                if (!File.Exists(pluginPath))
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {plugin.Key} doesn't exist. Running plugins maintenance (\"p m\" command is recommended).", ConsoleColor.Yellow);
                    continue;
                }
                
                var currentHash = Sha.Sha256File(pluginPath);
                string? latestVersion = null;
                
                if (Core.LocalAdmin.DataJson!.PluginVersionCache!.ContainsKey(plugin.Key))
                    latestVersion = Core.LocalAdmin.DataJson.PluginVersionCache[plugin.Key].Version;

                List<string>? dependencies = null;

                foreach (var dep in metadata.Dependencies)
                {
                    if (!dep.Value.InstalledByPlugins.Contains(plugin.Key))
                        continue;

                    dependencies ??= new();
                    dependencies.Add(dep.Key);
                }
                
                plugins.Add(new PluginListEntry(plugin.Key, plugin.Value.CurrentVersion, plugin.Value.TargetVersion, latestVersion, currentHash == plugin.Value.FileHash, dependencies));
            }

            return plugins;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to list plugins for port {port}! Exception: {e.Message}",
                ConsoleColor.Red);
            return null;
        }
    }

    internal readonly struct PluginListEntry
    {
        internal readonly string Name;
        internal readonly string InstalledVersion;
        internal readonly string TargetVersion;
        internal readonly string LatestVersion;
        internal readonly bool IntegrityCheckPassed;
        internal readonly List<string>? Dependencies;
        
        internal PluginListEntry(string name, string? installedVersion, string? targetVersion, string? latestVersion, bool integrityCheckPassed, List<string>? dependencies)
        {
            Name = name;
            InstalledVersion = installedVersion ?? "(null)";
            TargetVersion = targetVersion ?? "(null)";
            LatestVersion = latestVersion ?? "(null)";
            IntegrityCheckPassed = integrityCheckPassed;
            Dependencies = dependencies;
        }

        internal bool UpToDate => InstalledVersion.Equals(LatestVersion, StringComparison.Ordinal);

        internal bool FixedVersion =>
            TargetVersion != null && !TargetVersion.Equals("latest", StringComparison.OrdinalIgnoreCase);

        internal string InstalledVersionValidated =>
            IntegrityCheckPassed ? InstalledVersion : "UNKNOWN - manually modified";
    }
}
