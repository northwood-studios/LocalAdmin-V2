using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LocalAdmin.V2.Core;

internal class DataJson
{
    [JsonProperty("EulaAccepted")]
    public DateTime? EulaAccepted;

    [JsonProperty("PluginVersionCache")]
    public Dictionary<string, PluginVersionCache>? PluginVersionCache = new();
}

internal struct PluginVersionCache
{
    [JsonProperty("Version")]
    public string Version;

    [JsonProperty("ReleaseId")]
    public uint ReleaseId;

    [JsonProperty("PublishmentTime")]
    public DateTime PublishmentTime;
    
    [JsonProperty("LastRefreshed")]
    public DateTime LastRefreshed;

    [JsonProperty("DllDownloadUrl")]
    public string DllDownloadUrl;
    
    [JsonProperty("DependenciesDownloadUrl")]
    public string? DependenciesDownloadUrl;
}
