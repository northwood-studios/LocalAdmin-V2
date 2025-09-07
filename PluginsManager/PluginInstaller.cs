using LocalAdmin.V2.Core;
using LocalAdmin.V2.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Utf8Json;

namespace LocalAdmin.V2.PluginsManager;

internal static class PluginInstaller
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(45),
        DefaultRequestHeaders = { { "User-Agent", "LocalAdmin (SCP: Secret Laboratory Dedicated Server Tool)" } }
    };

    internal static void RefreshPat() => HttpClient.DefaultRequestHeaders.Authorization =
        Core.LocalAdmin.DataJson!.GitHubPersonalAccessToken == null
            ? null
            : new AuthenticationHeaderValue("Bearer", Core.LocalAdmin.DataJson.GitHubPersonalAccessToken);

    internal static string MetadataPath() => $"{PathManager.LabApiRoot}metadata-{Core.LocalAdmin.GamePort}.json";
    internal static string PluginsPath(string path) => $"{PathManager.PluginsPath}{PluginPaths.FormatPath(path)}{Path.DirectorySeparatorChar}";
    private static string DependenciesPath(string path) => $"{PathManager.DependenciesPath}{PluginPaths.FormatPath(path)}{Path.DirectorySeparatorChar}";
    private static string TempPath(ushort port) => $"{PathManager.GameUserDataRoot}internal{Path.DirectorySeparatorChar}LA Temp{Path.DirectorySeparatorChar}{port}{Path.DirectorySeparatorChar}";

    internal const uint DefaultLockTime = 30000;

    private static async Task<QueryResult> QueryRelease(string name, string url, bool interactive)
    {
        try
        {
            using var response = await HttpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to query {url}! Is the GitHub Personal Access Token set correctly? (Status code: {response.StatusCode})", ConsoleColor.Red);
                return new();
            }

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to query {url}! (Status code: {response.StatusCode})", ConsoleColor.Red);
                return new();
            }

            var data = JsonSerializer.Deserialize<GitHubRelease>(await response.Content.ReadAsStringAsync());

            if (data.tag_name == null)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - response is null.", ConsoleColor.Red);

                return new();
            }

            if (data.message != null)
            {
                if (response.StatusCode == HttpStatusCode.NotFound || data.message.Equals("Not Found", StringComparison.Ordinal))
                {
                    if (interactive)
                        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - plugin release not found or no public release/specified version found.", ConsoleColor.Red);
                    return new();
                }

                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name}. Exception: {data.message}", ConsoleColor.Red);
                return new();
            }

            if (data.assets == null || data.assets.Count == 0)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - no assets found.", ConsoleColor.Red);
                return new();
            }

            string? pluginUrl = null;
            string? dependenciesUrl = null;

            var designatedForNwApi = false;
            var nonNwApiFound = 0;

            foreach (var asset in data.assets)
            {
                if (asset.name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    var thisNw = asset.name.EndsWith("-nw.dll", StringComparison.OrdinalIgnoreCase);

                    if (designatedForNwApi)
                    {
                        if (!thisNw)
                            continue;

                        if (interactive)
                            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - multiple plugin DLLs marked for NW API usage found.", ConsoleColor.Red);
                        return new();
                    }

                    if (thisNw)
                        nonNwApiFound = 0;
                    else
                        nonNwApiFound++;

                    pluginUrl = asset.url;
                    designatedForNwApi = thisNw;
                }
                else if (asset.name.Equals("dependencies-nw.zip", StringComparison.OrdinalIgnoreCase))
                    dependenciesUrl = asset.url;
                else if (dependenciesUrl == null && asset.name.Equals("dependencies.zip", StringComparison.OrdinalIgnoreCase))
                    dependenciesUrl = asset.url;
            }

            if (pluginUrl == null)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - no plugin DLL found.", ConsoleColor.Red);
                return new();
            }

            if (nonNwApiFound > 1)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - multiple matching plugin DLLs found, none is explicitly designated for NW API usage.", ConsoleColor.Red);
                return new();
            }

            return new(new PluginVersionCache
            {
                Version = data.tag_name!,
                ReleaseId = data.id,
                DependenciesDownloadUrl = dependenciesUrl,
                DllDownloadUrl = pluginUrl,
                LastRefreshed = DateTime.UtcNow,
                PublishmentTime = data.published_at
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

    internal static async Task<bool> TryInstallPlugin(string name, PluginVersionCache plugin, string targetVersion, string plPath, string dependenciesPath, bool overwriteFiles, bool ignoreLocks)
    {
        var tempPath = TempPath(Core.LocalAdmin.GamePort);

        try
        {
            var pluginsPath = PluginsPath(plPath);
            var depPath = DependenciesPath(dependenciesPath);

            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);

            if (!Directory.Exists(depPath))
                Directory.CreateDirectory(depPath);

            var safeName = name.Replace("/", "_", StringComparison.Ordinal);
            var metadataPath = MetadataPath();
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

                metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : DefaultLockTime);

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

            List<string> currentDependencies = new();

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
                    metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : DefaultLockTime, true);
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Processing dependencies for plugin {name}...",
                        ConsoleColor.Blue);

                    var deps = Directory.GetFiles(extractDir, "*.dll", SearchOption.AllDirectories);

                    foreach (var dep in deps)
                    {
                        var fn = Path.GetFileName(dep);
                        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Processing dependency {fn}...", ConsoleColor.Blue);

                        currentDependencies.Add(fn);
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

                                ConsoleUtil.WriteLine(
                                    $"[PLUGIN MANAGER] Dependency {fn} is already installed, but not registered in metadata file! Adding to metadata file...",
                                    ConsoleColor.Yellow);
                            }

                            metadata.Dependencies.Add(fn, new Dependency
                            {
                                FileHash = newHash,
                                FilePath = dependenciesPath,
                                InstallationDate = DateTime.UtcNow,
                                UpdateDate = DateTime.UtcNow,
                                InstalledByPlugins = usedBy,
                                ManuallyInstalled = installed
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
                                metadata.Dependencies[fn].FilePath = dependenciesPath;
                                metadata.Dependencies[fn].UpdateDate = DateTime.UtcNow;
                            }

                            if (!metadata.Dependencies[fn].InstalledByPlugins.Contains(name))
                                metadata.Dependencies[fn].InstalledByPlugins.Add(name);

                            if (overwrite)
                            {
                                File.Move(dep, depPath + fn, true);
                                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Installed dependency {fn}.",
                                    ConsoleColor.Blue);
                            }
                            else
                                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Dependency {fn} is already installed.",
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
            var runMaintenance = false;

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
                metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : DefaultLockTime, true);

                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Processing metadata...", ConsoleColor.Blue);
                if (metadata!.InstalledPlugins.ContainsKey(name))
                {
                    metadata.InstalledPlugins[name].FileHash = hash;
                    metadata.InstalledPlugins[name].FilePath = plPath;
                    metadata.InstalledPlugins[name].UpdateDate = DateTime.UtcNow;
                    metadata.InstalledPlugins[name].CurrentVersion = plugin.Version;
                    metadata.InstalledPlugins[name].TargetVersion = targetVersion;
                }
                else
                    metadata.InstalledPlugins.Add(name, new InstalledPlugin
                    {
                        FileHash = hash,
                        FilePath = plPath,
                        InstallationDate = DateTime.UtcNow,
                        UpdateDate = DateTime.UtcNow,
                        CurrentVersion = plugin.Version,
                        TargetVersion = targetVersion
                    });

                foreach (var dependency in metadata.Dependencies)
                {
                    if (dependency.Value.InstalledByPlugins.Contains(name) &&
                        !currentDependencies.Contains(dependency.Key))
                    {
                        metadata.Dependencies[dependency.Key].InstalledByPlugins.Remove(name);
                        ConsoleUtil.WriteLine(
                            $"[PLUGIN MANAGER] Dependency {dependency.Key} is no longer needed by plugin {name}.",
                            ConsoleColor.Blue);

                        if (!dependency.Value.ManuallyInstalled &&
                            metadata.Dependencies[dependency.Key].InstalledByPlugins.Count == 0)
                        {
                            runMaintenance = true;

                            ConsoleUtil.WriteLine(
                                $"[PLUGIN MANAGER] Dependency {dependency.Key} is no longer needed by any plugin. Maintenance will be performed.",
                                ConsoleColor.Blue);
                        }
                    }
                }
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

            if (runMaintenance)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Performing automatic maintenance...", ConsoleColor.Blue);
                await PluginsMaintenance(false);
            }

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
        var octetStreamHeader = new MediaTypeWithQualityHeaderValue("application/octet-stream");
        await using var fs = File.OpenWrite(targetPath);
        HttpClient.DefaultRequestHeaders.Accept.Add(octetStreamHeader);

        try
        {
            using var response = await HttpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to query {url}! Is the GitHub Personal Access Token set correctly? (Status code: {response.StatusCode})", ConsoleColor.Red);
                return false;
            }

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

            HttpClient.DefaultRequestHeaders.Accept.Remove(octetStreamHeader);

            if (!success)
                File.Delete(targetPath);
        }
    }

    internal static async Task<bool> TryUninstallPlugin(string name, bool ignoreLocks, bool skipUpdate)
    {
        var performUpdate = OfficialPluginsList.IsRefreshNeeded();

        if (skipUpdate)
            performUpdate = false;

        if (performUpdate)
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Refreshing plugins list...", ConsoleColor.Yellow);
            await OfficialPluginsList.RefreshOfficialPluginsList();
        }

        name = OfficialPluginsList.ResolvePluginAlias(name, PluginAliasFlags.All);

        if (name.Count(x => x == '/') != 1)
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugin name is invalid!", ConsoleColor.Red);
            return false;
        }

        ServerPluginsConfig? metadata = null;
        var success = false;
        var metadataPath = MetadataPath();

        try
        {
            if (!File.Exists(metadataPath))
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Metadata file does not exist.", ConsoleColor.Red);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Uninstallation failed.", ConsoleColor.Red);
                return false;
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading metadata...", ConsoleColor.Blue);
            metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : DefaultLockTime, true);

            if (metadata == null)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to read metadata (null)!", ConsoleColor.Red);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Uninstallation failed.", ConsoleColor.Red);
                return false;
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Processing metadata...", ConsoleColor.Blue);
            if (metadata.InstalledPlugins.ContainsKey(name))
            {
                var safeName = name.Replace("/", "_", StringComparison.Ordinal);
                var pluginPath = PluginsPath(metadata.InstalledPlugins[name].FilePath ?? string.Empty) + $"{safeName}.dll";

                try
                {
                    if (FileUtils.DeleteIfExists(pluginPath))
                        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugin DLL deleted.", ConsoleColor.Blue);
                    else ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugin DLL does not exist.", ConsoleColor.Yellow);

                    metadata.InstalledPlugins.Remove(name);
                    await metadata.TrySave(metadataPath, 0);
                }
                catch (Exception e)
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to delete plugin {name}! Exception: {e.Message}",
                        ConsoleColor.Red);
                    return false;
                }
            }

            Dictionary<string, Dependency> depToRemove = new();

            foreach (var dep in metadata.Dependencies)
            {
                if (dep.Value.InstalledByPlugins.Contains(name))
                    dep.Value.InstalledByPlugins.Remove(name);

                if (dep.Value.InstalledByPlugins.Count == 0 && !dep.Value.ManuallyInstalled)
                    depToRemove.Add(dep.Key, dep.Value);
            }

            foreach (var dep in depToRemove)
            {
                try
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Removing redundant dependency {dep}...",
                        ConsoleColor.Blue);

                    if (FileUtils.DeleteIfExists(DependenciesPath(dep.Value.FilePath ?? string.Empty) + dep))
                        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Dependency deleted.", ConsoleColor.Blue);
                    else ConsoleUtil.WriteLine("[PLUGIN MANAGER] Dependency does not exist.", ConsoleColor.Yellow);

                    metadata.Dependencies.Remove(dep.Key);
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

    internal static async Task<bool> PluginsMaintenance(bool ignoreLocks)
    {
        ServerPluginsConfig? metadata = null;
        var success = false;
        var metadataPath = MetadataPath();

        try
        {
            if (!File.Exists(metadataPath))
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Metadata file for port {Core.LocalAdmin.GamePort} doesn't exist. No need to perform maintenance.", ConsoleColor.Blue);
                success = true;
                return true;
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading metadata...", ConsoleColor.Blue);
            metadata = await JsonFile.Load<ServerPluginsConfig>(metadataPath, ignoreLocks ? 0 : DefaultLockTime, true);

            if (metadata == null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to parse metadata file for port {Core.LocalAdmin.GamePort}!", ConsoleColor.Red);
                return false;
            }

            Dictionary<string, Dependency> depToRemove = new();
            Dictionary<string, InstalledPlugin> plToRemove = new();

            foreach (var pl in metadata.InstalledPlugins)
            {
                var pluginPath = PluginsPath(pl.Value.FilePath ?? string.Empty) + $"{pl.Key.Replace("/", "_", StringComparison.Ordinal)}.dll";

                if (File.Exists(pluginPath))
                    continue;

                plToRemove.Add(pl.Key, pl.Value);
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {pl.Key} has been manually removed.", ConsoleColor.Blue);
            }

            foreach (var pl in plToRemove.Keys)
                metadata.InstalledPlugins.Remove(pl);

            foreach (var dep in metadata.Dependencies)
            {
                if (!File.Exists(DependenciesPath(dep.Value.FilePath ?? string.Empty) + dep.Key))
                {
                    depToRemove.Add(dep.Key, dep.Value);
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Dependency {dep.Key} has been manually removed.", ConsoleColor.Blue);
                    continue;
                }

                plToRemove.Clear();
                foreach (var pl in dep.Value.InstalledByPlugins)
                {
                    if (metadata.InstalledPlugins.ContainsKey(pl))
                        continue;

                    plToRemove.Add(pl, null!); // We can put null here, only keys are used later
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Removed non-existing plugin {pl} from dependency {dep.Key}.", ConsoleColor.Blue);
                }

                foreach (var pl in plToRemove.Keys)
                    metadata.Dependencies[dep.Key].InstalledByPlugins.Remove(pl);

                if (dep.Value.InstalledByPlugins.Count == 0 && !dep.Value.ManuallyInstalled)
                    depToRemove.Add(dep.Key, dep.Value);
            }

            foreach (var dep in depToRemove)
            {
                try
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Removing redundant dependency {dep}...",
                        ConsoleColor.Blue);

                    ConsoleUtil.WriteLine(
                        FileUtils.DeleteIfExists(DependenciesPath(dep.Value.FilePath ?? string.Empty) + dep)
                            ? "[PLUGIN MANAGER] Dependency deleted."
                            : "[PLUGIN MANAGER] Dependency does not exist.", ConsoleColor.Blue);

                    metadata.Dependencies.Remove(dep.Key);
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
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to perform maintenance for port {Core.LocalAdmin.GamePort}! Exception: {e.Message}",
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
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugins maintenance for port {Core.LocalAdmin.GamePort} complete!", ConsoleColor.DarkGreen);
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