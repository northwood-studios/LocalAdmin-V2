using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace LocalAdmin.V2.JSON.Objects;

public readonly record struct GitHubRelease(
    string? message,
    uint id,
    [property: JsonPropertyName("tag_name")] string? tagName,
    [property: JsonPropertyName("published_at")] DateTime publishedAt,
    List<GitHubReleaseAsset> assets);

public readonly record struct GitHubReleaseAsset(string name, string url);