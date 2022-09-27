using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LocalAdmin.V2.PluginsManager;

internal class GitHubRelease
{
    [JsonProperty("message")]
    public string? Message;
    
    [JsonProperty("id")]
    public uint Id;
    
    [JsonProperty("tag_name")]
    public string? TagName;
    
    [JsonProperty("published_at")]
    public DateTime PublishmentTime;

    [JsonProperty("assets")]
    public List<GitHubReleaseAsset>? Assets = new();
}

internal struct GitHubReleaseAsset
{
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("browser_download_url")]
    public string DownloadUrl;
}
