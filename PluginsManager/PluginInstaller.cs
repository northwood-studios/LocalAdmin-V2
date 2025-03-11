using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.JSON;
using LocalAdmin.V2.JSON.Objects;

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

    internal static string PluginsPath(string port) => $"{PathManager.GameUserDataRoot}PluginAPI{Path.DirectorySeparatorChar}plugins{Path.DirectorySeparatorChar}{port}{Path.DirectorySeparatorChar}";
    private static string DependenciesPath(string port) => $"{PluginsPath(port)}dependencies{Path.DirectorySeparatorChar}";
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
                return new QueryResult();
            }

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to query {url}! (Status code: {response.StatusCode})", ConsoleColor.Red);
                return new QueryResult();
            }

            var data = await response.Content.ReadFromJsonAsync(JsonGenerated.Default.GitHubRelease);

            if (data.tagName == null)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - response is null.", ConsoleColor.Red);

                return new QueryResult();
            }

            if (data.message != null)
            {
                if (response.StatusCode == HttpStatusCode.NotFound || data.message.Equals("Not Found", StringComparison.Ordinal))
                {
                    if (interactive)
                        ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - plugin release not found or no public release/specified version found.", ConsoleColor.Red);
                    return new QueryResult();
                }

                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name}. Exception: {data.message}", ConsoleColor.Red);
                return new QueryResult();
            }

            if (data.assets == null || data.assets.Count == 0)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - no assets found.", ConsoleColor.Red);
                return new QueryResult();
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
                        return new QueryResult();
                    }

                    if (thisNw)
                        nonNwApiFound = 0;
                    else
                        nonNwApiFound++;

                    pluginUrl = asset.url;
                    designatedForNwApi = thisNw;
                }
                else if (asset.name.Equals("dependencies-nw.zip", StringComparison.OrdinalIgnoreCase) ||
                         dependenciesUrl == null && asset.name.Equals("dependencies.zip", StringComparison.OrdinalIgnoreCase))
                    dependenciesUrl = asset.url;
            }

            if (pluginUrl == null)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - no plugin DLL found.", ConsoleColor.Red);
                return new QueryResult();
            }

            if (nonNwApiFound > 1)
            {
                if (interactive)
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {name} - multiple matching plugin DLLs found, none is explicitly designated for NW API usage.", ConsoleColor.Red);
                return new QueryResult();
            }

            return new QueryResult(new PluginVersionCache
            {
                Version = data.tagName!,
                ReleaseId = data.id,
                DependenciesDownloadUrl = dependenciesUrl,
                DllDownloadUrl = pluginUrl,
                LastRefreshed = DateTime.UtcNow,
                PublishmentTime = data.publishedAt
            });
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to process plugin {url}! Exception: {e.Message}", ConsoleColor.Red);
            return new QueryResult();
        }
    }

    internal static async Task<QueryResult> TryCachePlugin(string name, bool interactive)
    {
        var response = await QueryRelease(name, $"https://api.github.com/repos/{name}/releases/latest", interactive);

        if (!response.Success)
            return response;

        Core.LocalAdmin.DataJson!.PluginVersionCache[name] = response.Result;

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

            Directory.CreateDirectory(pluginsPath);
            Directory.CreateDirectory(depPath);

            var safeName = name.Replace("/", "_", StringComparison.Ordinal);
            var metadataPath = $"{pluginsPath}metadata.json";
            var pluginPath = $"{pluginsPath}{safeName}.dll";

            uint timeout = ignoreLocks ? 0 : DefaultLockTime;
            await using FileStream fileStream = await FileUtils.OpenAsync(metadataPath, FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None, timeout);

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Loading metadata file...", ConsoleColor.Blue);
            ServerPluginsConfig metadata = (fileStream.Length != 0 ?
                await JsonSerializer.DeserializeAsync<ServerPluginsConfig>(fileStream, JsonGenerated.Default.ServerPluginsConfig) : null) ??
                new ServerPluginsConfig([], [], null);

            if (!overwriteFiles)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Checking if plugin is already installed...", ConsoleColor.Blue);

                if (metadata.InstalledPlugins.TryGetValue(name, out InstalledPlugin? installedPlugin))
                {
                    if (installedPlugin.CurrentVersion == plugin.Version)
                    {
                        ConsoleUtil.WriteLine(
                            $"[PLUGIN MANAGER] Plugin {name} is already installed (in this version)! Skipping...",
                            ConsoleColor.Yellow);

                        return true;
                    }
                }
            }

            Directory.CreateDirectory(tempPath);

            List<string> currentDependencies = [];

            if (plugin.DependenciesDownloadUrl != null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Downloading dependencies for plugin {name}...",
                    ConsoleColor.Blue);

                var extractDir = $"{tempPath}{safeName}-dependencies";

                string targetPath = $"{tempPath}{safeName}-dependencies.zip";
                try
                {
                    bool dwlOk = await Download($"{name}-dependencies", plugin.DependenciesDownloadUrl,
                        targetPath);

                    if (!dwlOk)
                    {
                        ConsoleUtil.WriteLine(
                            $"[PLUGIN MANAGER] Failed to download plugin dependencies {name}! Aborting download!",
                            ConsoleColor.Red);
                        return false;
                    }

                    FileUtils.DeleteDirectoryIfExists(extractDir);

                    Directory.CreateDirectory(extractDir);

                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Unpacking dependencies for plugin {name}...",
                        ConsoleColor.Blue);
                    ZipFile.ExtractToDirectory(targetPath, extractDir);

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

                        if (!installed)
                            metadata.Dependencies.Remove(fn);

                        if (!metadata.Dependencies.TryGetValue(fn, out Dependency? depMeta))
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

                            metadata.Dependencies.Add(fn,
                                new Dependency(FileHash: newHash, InstallationDate: DateTime.UtcNow,
                                    UpdateDate: DateTime.UtcNow, InstalledByPlugins: usedBy,
                                    ManuallyInstalled: installed));

                            File.Move(dep, depPath + fn, true);
                            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Installed dependency {fn}.", ConsoleColor.Blue);
                        }
                        else
                        {
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
                                metadata.Dependencies[fn] = metadata.Dependencies[fn] with { FileHash = newHash, UpdateDate = DateTime.UtcNow };
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
                    ConsoleUtil.WriteLine("[PLUGIN MANAGER] Cleaning up...", ConsoleColor.Blue);
                    FileUtils.DeleteDirectoryIfExists(extractDir);
                    FileUtils.DeleteIfExists(targetPath);
                }
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

                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Processing metadata...", ConsoleColor.Blue);
                metadata.InstalledPlugins[name] = new InstalledPlugin
                (
                    targetVersion,
                    plugin.Version,
                    hash,
                    metadata.InstalledPlugins.TryGetValue(name, out InstalledPlugin? value) ? value.InstallationDate : DateTime.UtcNow,
                    DateTime.UtcNow
                );

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

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);

            fileStream.SetLength(0);
            await JsonSerializer.SerializeAsync(fileStream, metadata, JsonGenerated.Default.ServerPluginsConfig);

            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {name} has been successfully installed!", ConsoleColor.DarkGreen);

            if (runMaintenance)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Performing automatic maintenance...", ConsoleColor.Blue);
                await PluginsMaintenance(port, false);
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

    internal static async Task<bool> TryUninstallPlugin(string name, string port, bool ignoreLocks, bool skipUpdate)
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

        if (name.AsSpan().Count('/') != 1)
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Plugin name is invalid!", ConsoleColor.Red);
            return false;
        }

        var pluginsPath = PluginsPath(port);

        Directory.CreateDirectory(pluginsPath);

        var metadataPath = $"{PluginsPath(port)}metadata.json";

        try
        {
            var depPath = DependenciesPath(port);

            Directory.CreateDirectory(depPath);

            var safeName = name.Replace("/", "_", StringComparison.Ordinal);

            var pluginPath = $"{PluginsPath(port)}{safeName}.dll";

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

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading metadata...", ConsoleColor.Blue);
            uint timeout = ignoreLocks ? 0 : DefaultLockTime;

            await using FileStream? fileStream =
                await FileUtils.TryOpenAsync(metadataPath, FileAccess.ReadWrite, FileShare.None, timeout);

            if (fileStream == null)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Metadata file does not exist.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Uninstallation complete.", ConsoleColor.Blue);
                return true;
            }

            var metadata = await JsonSerializer.DeserializeAsync<ServerPluginsConfig>(fileStream, JsonGenerated.Default.ServerPluginsConfig);

            if (metadata == null)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to read metadata (null)!", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Uninstallation complete.", ConsoleColor.Blue);
                return true;
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Processing metadata...", ConsoleColor.Blue);
            metadata.InstalledPlugins.Remove(name);

            List<string> depToRemove = [];

            foreach (var dep in metadata.Dependencies)
            {
                dep.Value.InstalledByPlugins.Remove(name);

                if (dep.Value.InstalledByPlugins.Count == 0 && !dep.Value.ManuallyInstalled)
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

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);

            fileStream.SetLength(0);
            await JsonSerializer.SerializeAsync(fileStream, metadata, JsonGenerated.Default.ServerPluginsConfig);

            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {name} has been successfully uninstalled!", ConsoleColor.DarkGreen);
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to remove plugin {name}! Exception: {e.Message}",
                ConsoleColor.Red);
            return false;
        }
    }

    internal static async Task<bool> PluginsMaintenance(string port, bool ignoreLocks)
    {
        var pluginsPath = PluginsPath(port);

        if (!Directory.Exists(pluginsPath))
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugins path for port {port} doesn't exist. No need to perform maintenance.", ConsoleColor.Blue);
            return true;
        }

        var depPath = DependenciesPath(port);

        Directory.CreateDirectory(depPath);

        var metadataPath = $"{pluginsPath}metadata.json";

        try
        {
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading metadata...", ConsoleColor.Blue);

            uint timeout = ignoreLocks ? 0 : DefaultLockTime;
            await using FileStream? fileStream = await FileUtils.TryOpenAsync(metadataPath, FileAccess.ReadWrite, FileShare.None, timeout);

            if (fileStream == null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Metadata file for port {port} doesn't exist. No need to perform maintenance.", ConsoleColor.Blue);
                return true;
            }

            ServerPluginsConfig? metadata = await JsonSerializer.DeserializeAsync<ServerPluginsConfig>(fileStream, JsonGenerated.Default.ServerPluginsConfig);

            if (metadata == null)
            {
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to parse metadata file for port {port}!", ConsoleColor.Red);
                return false;
            }

            List<string> depToRemove = [], plToRemove = [];

            foreach (var pl in metadata.InstalledPlugins)
            {
                var pluginPath = $"{pluginsPath}{pl.Key.Replace("/", "_", StringComparison.Ordinal)}.dll";

                if (File.Exists(pluginPath))
                    continue;

                plToRemove.Add(pl.Key);
                ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugin {pl.Key} has been manually removed.", ConsoleColor.Blue);
            }

            foreach (var pl in plToRemove)
                metadata.InstalledPlugins.Remove(pl);

            foreach (var dep in metadata.Dependencies)
            {
                if (!File.Exists(depPath + dep.Key))
                {
                    depToRemove.Add(dep.Key);
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Dependency {dep.Key} has been manually removed.", ConsoleColor.Blue);
                    continue;
                }

                plToRemove.Clear();
                foreach (var pl in dep.Value.InstalledByPlugins)
                {
                    if (metadata.InstalledPlugins.ContainsKey(pl))
                        continue;

                    plToRemove.Add(pl);
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Removed non-existing plugin {pl} from dependency {dep.Key}.", ConsoleColor.Blue);
                }

                foreach (var pl in plToRemove)
                    metadata.Dependencies[dep.Key].InstalledByPlugins.Remove(pl);

                if (dep.Value.InstalledByPlugins.Count == 0 && !dep.Value.ManuallyInstalled)
                    depToRemove.Add(dep.Key);
            }

            foreach (var dep in depToRemove)
            {
                try
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Removing redundant dependency {dep}...",
                        ConsoleColor.Blue);

                    ConsoleUtil.WriteLine(
                        FileUtils.DeleteIfExists(depPath + dep)
                            ? "[PLUGIN MANAGER] Dependency deleted."
                            : "[PLUGIN MANAGER] Dependency does not exist.", ConsoleColor.Blue);

                    metadata.Dependencies.Remove(dep);
                }
                catch (Exception e)
                {
                    ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to delete dependency {dep}! Exception: {e.Message}",
                        ConsoleColor.Yellow);
                }
            }

            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing metadata...", ConsoleColor.Blue);

            fileStream.SetLength(0);
            await JsonSerializer.SerializeAsync(fileStream, metadata, JsonGenerated.Default.ServerPluginsConfig);

            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Plugins maintenance for port {port} complete!", ConsoleColor.DarkGreen);
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"[PLUGIN MANAGER] Failed to perform maintenance for port {port}! Exception: {e.Message}",
                ConsoleColor.Red);
            return false;
        }
    }

    internal readonly record struct QueryResult(PluginVersionCache Result)
    {
        public readonly bool Success = true;
        public readonly PluginVersionCache Result = Result;
    }
}