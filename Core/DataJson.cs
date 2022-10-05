using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LocalAdmin.V2.Core;

internal class DataJson
{
    [JsonProperty("GitHubPersonalAccessToken")]
    public string? GitHubPersonalAccessToken;
    
    [JsonProperty("EulaAccepted")]
    public DateTime? EulaAccepted;

    [JsonProperty("PluginManagerWarningDismissed")]
    public bool PluginManagerWarningDismissed;

    [JsonProperty("LastPluginAliasesRefresh")]
    public DateTime? LastPluginAliasesRefresh;

    [JsonProperty("PluginVersionCache")]
    public Dictionary<string, PluginVersionCache>? PluginVersionCache = new();
    
    [JsonProperty("PluginAliases")]
    public Dictionary<string, PluginAlias>? PluginAliases = new();
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

internal struct PluginAlias
{
    [JsonProperty("Repository")]
    public string Repository;
    
    [JsonProperty("Flags")]
    public PluginAliasFlags Flags;
}

[Flags]
internal enum PluginAliasFlags : byte
{
    None = 0,
    Listed = 1,
    CanInstall = 1 << 1,
    All = Listed | CanInstall
}
