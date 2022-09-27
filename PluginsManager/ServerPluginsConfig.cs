using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LocalAdmin.V2.PluginsManager;

public class ServerPluginsConfig
{
    [JsonProperty("InstalledPlugins")]
    internal Dictionary<string, InstalledPlugin> InstalledPlugins = new();
    
    [JsonProperty("Dependencies")]
    internal Dictionary<string, Dependency> Dependencies = new();
}

internal class InstalledPlugin
{
    [JsonProperty("TargetVersion")]
    public string? TargetVersion;

    [JsonProperty("CurrentVersion")]
    public string? CurrentVersion;

    [JsonProperty("FileHash")]
    public string? FileHash;

    [JsonProperty("InstallationDate")]
    public DateTime InstallationDate;

    [JsonProperty("UpdateDate")]
    public DateTime UpdateDate;
}

internal class Dependency
{
    [JsonProperty("FileHash")]
    public string? FileHash;
    
    [JsonProperty("InstallationDate")]
    public DateTime InstallationDate;

    [JsonProperty("UpdateDate")]
    public DateTime UpdateDate;

    [JsonProperty("InstalledByPlugins")]
    public List<string> InstalledByPlugins = new();
}
