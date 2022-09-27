using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LocalAdmin.V2.Core;
using LocalAdmin.V2.IO;
using Newtonsoft.Json;

namespace LocalAdmin.V2.PluginsManager;

internal static class PluginInstaller
{
    private static readonly HttpClient HttpClient = new ()
    {
        Timeout = TimeSpan.FromSeconds(45),
        DefaultRequestHeaders = { { "User-Agent", "LocalAdmin (SCP: Secret Laboratory Dedicated Server Tool)" } }
    };
    
    private static string PluginsPath(string port) => $"{PathManager.GameUserDataRoot}config{Path.DirectorySeparatorChar}PluginAPI{Path.DirectorySeparatorChar}plugins{Path.DirectorySeparatorChar}{port}{Path.DirectorySeparatorChar}";
    private static string DependenciesPath(string port) => $"{PluginsPath(port)}dependencies{Path.DirectorySeparatorChar}";
    private static string TempPath(ushort port) => $"{PathManager.GameUserDataRoot}internal{Path.DirectorySeparatorChar}LA Temp{Path.DirectorySeparatorChar}{port}{Path.DirectorySeparatorChar}";
    
    private static async Task<QueryResult> QueryRelease(string name, string url, bool interactive)
    {
        try
        {
            using var response = await HttpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to query {url}! (Status code: {response.StatusCode})", ConsoleColor.Red);
                return new();
            }
            
            var data = JsonConvert.DeserializeObject<GitHubRelease>(await response.Content.ReadAsStringAsync());

            if (data == null)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - response is null.", ConsoleColor.Red);
                
                return new();
            }
            
            if (data.Message != null)
            {
                if (response.StatusCode == HttpStatusCode.NotFound || data.Message.Equals("Not Found", StringComparison.Ordinal))
                {
                    if (interactive)
                        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - plugin release not found or no public release/specified version found.", ConsoleColor.Red);
                    return new();
                }
            
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name}. Exception: {data.Message}", ConsoleColor.Red);
                return new();
            }
            
            if (data.Assets == null || data.Assets.Count == 0)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - no assets found.", ConsoleColor.Red);
                return new();
            }
            
            string? pluginUrl = null;
            string? dependenciesUrl = null;
        
            foreach (var asset in data.Assets)
            {
                if (asset.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    if (pluginUrl != null)
                    {
                        if (interactive)
                            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - multiple plugin DLLs found.", ConsoleColor.Red);
                        return new();
                    }
                    
                    pluginUrl = asset.DownloadUrl;
                }
                else if (asset.Name.Equals("dependencies.zip", StringComparison.OrdinalIgnoreCase))
                    dependenciesUrl = asset.DownloadUrl;
            }
        
            if (pluginUrl == null)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - no plugin DLL found.", ConsoleColor.Red);
                return new();
            }
            
            return new(new PluginVersionCache
            {
                Version = data.TagName!,
                ReleaseId = data.Id,
                DependenciesDownloadUrl = dependenciesUrl,
                DllDownloadUrl = pluginUrl,
                LastRefreshed = DateTime.UtcNow,
                PublishmentTime = data.PublishmentTime
            });
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {url}! Exception: {e.Message}", ConsoleColor.Red);
            return new();
        }
    }

    internal static async Task<QueryResult> TryCachePlugin(string name, bool interactive)
    {
        var response = await QueryRelease(name, $"https://api.github.com/repos/{name}/releases/latest", interactive);
        
        if (!response.Success)
            return response;
        
        if (Core.LocalAdmin.DataJson!.PluginVersionCache!.ContainsKey(name))
            Core.LocalAdmin.DataJson.PluginVersionCache![name] = response.Result;
        else Core.LocalAdmin.DataJson.PluginVersionCache!.Add(name, response.Result);

        return response;
    }

    internal static async Task<QueryResult>
        TryGetVersionDetails(string name, string version, bool interactive = true) => await QueryRelease(name,
        $"https://api.github.com/repos/{name}/releases/tags/{version}", interactive);

    internal static async Task<bool> TryInstallPlugin(string name, PluginVersionCache plugin, string targetVersion, string port, bool overwriteFiles, bool ignoreLocks)
    {
        var tempPath = TempPath(Core.LocalAdmin.GamePort);
        
        try
        {
            var pluginsPath = PluginsPath(port);
            var depPath = DependenciesPath(port);

            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);
            
            if (!Directory.Exists(depPath))
                Directory.CreateDirectory(depPath);
            
            var safeName = name.Replace("/", "_", StringComparison.Ordinal);
            var metadataPath = pluginsPath + "metadata.json";
            var pluginPath = pluginsPath + $"{safeName}.dll";
            var abort = false;
            ServerPluginsConfig? metadata = null;

            if (!File.Exists(metadataPath))
            {
                var mt = new ServerPluginsConfig();
                bool ts = await mt.TrySave(metadataPath);

                if (!ts)
                {
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Could not create metadata file! Aborting download!",
                        ConsoleColor.Red);
                    return false;
                }
            }

            if (!overwriteFiles)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Checking if plugin is already installed...", ConsoleColor.Blue);

                metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? (uint)0 : 20000);
                
                if (metadata!.InstalledPlugins.ContainsKey(name))
                {
                    var installedPlugin = metadata.InstalledPlugins[name];
                    if (installedPlugin.CurrentVersion == plugin.Version)
                    {
                        ConsoleUtil.WriteLine(
                            $"[PLUGIN MANAGER] Plugin {name} is already installed (in this version)! Skipping...",
                            ConsoleColor.Yellow);
                        
                        return true;
                    }
                }

                metadata = null;
            }

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            if (plugin.DependenciesDownloadUrl != null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Downloading dependencies for plugin {name}...",
                    ConsoleColor.Blue);

                var extractDir = $"{tempPath}{safeName}-dependencies";

                try
                {
                    bool dwlOk = await Download(name + "-dependencies", plugin.DependenciesDownloadUrl,
                        $"{tempPath}{safeName}-dependencies.zip");

                    if (!dwlOk)
                    {
                        ConsoleUtil.WriteLine(
                            $"[PLUGIN MANAGER] Failed to download plugin dependencies {name}! Aborting download!",
                            ConsoleColor.Red);
                        return false;
                    }

                    if (Directory.Exists(extractDir))
                        Directory.Delete(extractDir, true);

                    Directory.CreateDirectory(extractDir);

                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Unpacking dependencies for plugin {name}...",
                        ConsoleColor.Blue);
                    ZipFile.ExtractToDirectory($"{tempPath}{safeName}-dependencies.zip", extractDir);

                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Loading metadata file...", ConsoleColor.Blue);
                    metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? (uint)0 : 20000, true);
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Processing dependencies for plugin {name}...",
                        ConsoleColor.Blue);

                    var deps = Directory.GetFiles(extractDir, "*.dll", SearchOption.AllDirectories);

                    foreach (var dep in deps)
                    {
                        var fn = Path.GetFileName(dep);
                        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Processing dependency {fn}...", ConsoleColor.Blue);

                        var installed = File.Exists(depPath + fn);
                        var newHash = Sha.Sha256File(dep);

                        if (!installed && metadata!.Dependencies.ContainsKey(fn))
                            metadata.Dependencies.Remove(fn);

                        if (!metadata!.Dependencies.ContainsKey(fn))
                        {
                            var usedBy = new List<string> { name };

                            if (installed)
                            {
                                if (newHash != Sha.Sha256File(depPath + fn))
                                {
                                    if (!overwriteFiles)
                                    {
                                        ConsoleUtil.WriteLine(
                                            $"[PLUGIN MANAGER] Dependency {fn} is already installed in a different version! To overwrite it run installation with -o arg.",
                                            ConsoleColor.Red);
                                        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Aborting download!", ConsoleColor.Red);
                                        return false;
                                    }

                                    ConsoleUtil.WriteLine(
                                        $"[PLUGIN MANAGER] Dependency {fn} is already installed in a different version! Overwriting...",
                                        ConsoleColor.Yellow);
                                }

                                usedBy.Add("unknown plugin");
                                ConsoleUtil.WriteLine(
                                    $"[PLUGIN MANAGER] Dependency {fn} is already installed, but not registered in metadata file! Adding to metadata file...",
                                    ConsoleColor.Yellow);
                            }

                            metadata.Dependencies.Add(fn, new Dependency
                            {
                                FileHash = newHash,
                                InstallationDate = DateTime.UtcNow,
                                UpdateDate = DateTime.UtcNow,
                                InstalledByPlugins = usedBy
                            });

                            File.Move(dep, depPath + fn, true);
                            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Installed dependency {fn}.", ConsoleColor.Blue);
                        }
                        else
                        {
                            var depMeta = metadata.Dependencies[fn];
                            var currentHash = Sha.Sha256File(depPath + fn);
                            var overwrite = false;

                            if (currentHash != depMeta.FileHash)
                            {
                                if (!overwriteFiles)
                                {
                                    ConsoleUtil.WriteLine(
                                        $"[PLUGIN MANAGER] Dependency {fn} has been manually modified! To overwrite it run installation with -o arg.",
                                        ConsoleColor.Red);
                                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Aborting download!", ConsoleColor.Red);
                                    return false;
                                }

                                ConsoleUtil.WriteLine(
                                    $"[PLUGIN MANAGER] Dependency {fn} has been manually modified! Overwriting...",
                                    ConsoleColor.Yellow);
                                overwrite = true;
                            }

                            if (newHash != depMeta.FileHash)
                            {
                                if (!overwriteFiles)
                                {
                                    ConsoleUtil.WriteLine(
                                        $"[PLUGIN MANAGER] Dependency {fn} is already installed in a different version! To overwrite it run installation with -o arg.",
                                        ConsoleColor.Red);
                                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Aborting download!", ConsoleColor.Red);
                                    return false;
                                }

                                ConsoleUtil.WriteLine(
                                    $"[PLUGIN MANAGER] Dependency {fn} is already installed in a different version! Overwriting...",
                                    ConsoleColor.Yellow);
                                overwrite = true;
                            }

                            if (overwrite)
                            {
                                metadata.Dependencies[fn].FileHash = newHash;
                                metadata.Dependencies[fn].UpdateDate = DateTime.UtcNow;
                            }

                            metadata.Dependencies[fn].InstalledByPlugins.Add(name);

                            if (overwrite)
                            {
                                File.Move(dep, depPath + fn, true);
                                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Installed dependency {fn}.",
                                    ConsoleColor.Blue);
                            }
                            else
                                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Dependency {fn} is already installed",
                                    ConsoleColor.Blue);
                        }
                    }
                }
                finally
                {
                    if (metadata != null)
                    {
                        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);
                        if (!await metadata.TrySave(metadataPath, 0, true))
                        {
                            abort = true;
                            ConsoleUtil.WriteLine(
                                "[PLUGIN MANAGER] Failed to save metadata. Aborting further installation!",
                                ConsoleColor.Red);
                        }

                        metadata = null;
                    }

                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Cleaning up...", ConsoleColor.Blue);
                    FileUtils.DeleteDirectoryIfExists(extractDir);
                    FileUtils.DeleteIfExists($"{tempPath}{safeName}-dependencies.zip");
                }

                if (abort)
                    return false;
            }
            
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Downloading plugin {name}...", ConsoleColor.Blue);

            try
            {
                bool dwlOk = await Download(name, plugin.DllDownloadUrl,
                    $"{tempPath}{safeName}.dll");

                if (!dwlOk)
                {
                    ConsoleUtil.WriteLine(
                        $"[PLUGIN MANAGER] Failed to download plugin {name}! Aborting download!", ConsoleColor.Red);
                    return false;
                }

                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Installing plugin {name}...", ConsoleColor.Blue);
                File.Move($"{tempPath}{safeName}.dll", pluginPath, true);

                var hash = Sha.Sha256File(pluginPath);

                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading metadata...", ConsoleColor.Blue);
                metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? (uint)0 : 20000, true);

                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Processing metadata...", ConsoleColor.Blue);
                if (metadata!.InstalledPlugins.ContainsKey(name))
                {
                    metadata.InstalledPlugins[name].FileHash = hash;
                    metadata.InstalledPlugins[name].UpdateDate = DateTime.UtcNow;
                    metadata.InstalledPlugins[name].CurrentVersion = plugin.Version;
                    metadata.InstalledPlugins[name].TargetVersion = targetVersion;
                }
                else
                    metadata.InstalledPlugins.Add(name, new InstalledPlugin
                    {
                        FileHash = hash,
                        InstallationDate = DateTime.UtcNow,
                        UpdateDate = DateTime.UtcNow,
                        CurrentVersion = plugin.Version,
                        TargetVersion = targetVersion
                    });
            }
            catch (Exception e)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to install plugin {name}! Exception: {e.Message}",
                    ConsoleColor.Red);
                return false;
            }
            finally
            {
                if (metadata != null)
                {
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);
                    if (!await metadata.TrySave(metadataPath, 0, true))
                    {
                        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to save metadata!", ConsoleColor.Red);
                        abort = true;
                    }
                }
            }

            if (abort)
                return false;

            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {name} has been successfully installed!", ConsoleColor.DarkGreen);
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to download and install plugin {name}! Exception: {e.Message}", ConsoleColor.Red);
            return false;
        }
        finally
        {
            try
            {
                FileUtils.DeleteDirectoryIfExists(tempPath);
            }
            catch (Exception e)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to delete temp directory {tempPath}! Exception: {e.Message}",
                    ConsoleColor.Red);
            }
        }
    }
    
    private static async Task<bool> Download(string name, string url, string targetPath)
    {
        var success = false;
        await using var fs = File.OpenWrite(targetPath);

        try
        {
            using var response = await HttpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to download plugin {name}! (Status code: {response.StatusCode})", ConsoleColor.Red);
                return false;
            }

            await response.Content.CopyToAsync(fs);
            success = true;
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to download plugin {name}! Exception: {e.Message}", ConsoleColor.Red);
            return false;
        }
        finally
        {
            await fs.FlushAsync();
            fs.Close();

            if (!success)
                File.Delete(targetPath);
        }
    }

    internal static async Task<bool> TryUninstallPlugin(string name, string port, bool ignoreLocks)
    {
        ServerPluginsConfig? metadata = null;
        var pluginsPath = PluginsPath(port);
        
        if (!Directory.Exists(pluginsPath))
            Directory.CreateDirectory(pluginsPath);

        bool success = false;
        
        var metadataPath = PluginsPath(port) + "metadata.json";

        try
        {
            var depPath = DependenciesPath(port);
            
            if (!Directory.Exists(depPath))
                Directory.CreateDirectory(depPath);

            var safeName = name.Replace("/", "_", StringComparison.Ordinal);
            
            var pluginPath = PluginsPath(port) + $"{safeName}.dll";

            try
            {
                if (FileUtils.DeleteIfExists(pluginPath))
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugin DLL deleted.", ConsoleColor.Blue);
                else ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugin DLL does not exist.", ConsoleColor.Yellow);
            }
            catch (Exception e)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to delete plugin {name}! Exception: {e.Message}",
                    ConsoleColor.Red);
                return false;
            }

            if (!File.Exists(metadataPath))
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Metadata file does not exist.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Uninstallation complete.", ConsoleColor.Blue);
                return true;
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading metadata...", ConsoleColor.Blue);
            metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? (uint)0 : 20000, true);

            if (metadata == null)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to read metadata (null)!", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Uninstallation complete.", ConsoleColor.Blue);
                return true;
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Processing metadata...", ConsoleColor.Blue);
            if (metadata.InstalledPlugins.ContainsKey(name))
            {
                metadata.InstalledPlugins.Remove(name);
                await metadata.TrySave(metadataPath, 0);
            }

            List<string> depToRemove = new();

            foreach (var dep in metadata.Dependencies)
            {
                if (dep.Value.InstalledByPlugins.Contains(name))
                    dep.Value.InstalledByPlugins.Remove(name);

                if (dep.Value.InstalledByPlugins.Count == 0)
                    depToRemove.Add(dep.Key);
            }

            foreach (var dep in depToRemove)
            {
                try
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Removing redundant dependency {dep}...",
                        ConsoleColor.Blue);

                    if (FileUtils.DeleteIfExists(depPath + dep))
                        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Dependency deleted.", ConsoleColor.Blue);
                    else ConsoleUtil.WriteLine("[PLUGIN MANAGER] Dependency does not exist.", ConsoleColor.Yellow);

                    metadata.Dependencies.Remove(dep);
                }
                catch (Exception e)
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to delete dependency {dep}! Exception: {e.Message}",
                        ConsoleColor.Yellow);
                }
            }

            success = true;
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to remove plugin {name}! Exception: {e.Message}",
                ConsoleColor.Red);
            return false;
        }
        finally
        {
            if (metadata != null)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);
                
                if (!await metadata.TrySave(metadataPath, 0, true))
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to save metadata!", ConsoleColor.Red);
            }
            
            if (success)
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {name} has been successfully uninstalled!", ConsoleColor.DarkGreen);
        }
    }
    
    internal readonly struct QueryResult
    {
        public QueryResult()
        {
            Success = false;
            Result = default;
        }
        
        public QueryResult(PluginVersionCache result)
        {
            Success = true;
            Result = result;
        }
        
        public readonly bool Success;
        public readonly PluginVersionCache Result;
    }
}