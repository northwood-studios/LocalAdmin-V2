using System;
using System.Collections.Generic;
using Utf8Json;
// ReSharper disable InconsistentNaming

namespace LocalAdmin.V2.PluginsManager;

public readonly struct GitHubRelease
{
    public readonly string? message;

    public readonly uint id;

    public readonly string? tag_name;

    public readonly DateTime published_at;

    public readonly List<GitHubReleaseAsset> assets;

    [SerializationConstructor]
    public GitHubRelease(string? message, uint id, string? tag_name, DateTime published_at, List<GitHubReleaseAsset> assets)
    {
        this.message = message;
        this.id = id;
        this.tag_name = tag_name;
        this.published_at = published_at;
        this.assets = assets;

        this.assets ??= new();
    }
}

public readonly struct GitHubReleaseAsset
{
    public readonly string name;

    public readonly string url;

    [SerializationConstructor]
    public GitHubReleaseAsset(string name, string url)
    {
        this.name = name;
        this.url = url;
    }
}