using System;
using System.Collections.Generic;
using Utf8Json;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace LocalAdmin.V2.Core;

public class DataJson
{
    public string? GitHubPersonalAccessToken;

    public DateTime? EulaAccepted;

    public bool PluginManagerWarningDismissed;

    public DateTime? LastPluginAliasesRefresh;

    public Dictionary<string, PluginVersionCache> PluginVersionCache;

    public Dictionary<string, PluginAlias> PluginAliases;

    internal DataJson()
    {
        PluginVersionCache = new();
        PluginAliases = new();
    }

    [SerializationConstructor]
    public DataJson(string? gitHubPersonalAccessToken, DateTime? eulaAccepted, bool pluginManagerWarningDismissed, DateTime? lastPluginAliasesRefresh, Dictionary<string, PluginVersionCache> pluginVersionCache, Dictionary<string, PluginAlias> pluginAliases)
    {
        GitHubPersonalAccessToken = gitHubPersonalAccessToken;
        EulaAccepted = eulaAccepted;
        PluginManagerWarningDismissed = pluginManagerWarningDismissed;
        LastPluginAliasesRefresh = lastPluginAliasesRefresh;
        PluginVersionCache = pluginVersionCache;
        PluginAliases = pluginAliases;

        PluginVersionCache ??= new();
        PluginAliases ??= new();
    }
}

public struct PluginVersionCache
{
    public string Version;

    public uint ReleaseId;

    public DateTime PublishmentTime;

    public DateTime LastRefreshed;

    public string DllDownloadUrl;

    public string? DependenciesDownloadUrl;

    [SerializationConstructor]
    public PluginVersionCache(string version, uint releaseId, DateTime publishmentTime, DateTime lastRefreshed, string dllDownloadUrl, string? dependenciesDownloadUrl)
    {
        Version = version;
        ReleaseId = releaseId;
        PublishmentTime = publishmentTime;
        LastRefreshed = lastRefreshed;
        DllDownloadUrl = dllDownloadUrl;
        DependenciesDownloadUrl = dependenciesDownloadUrl;
    }
}

public readonly struct PluginAlias
{
    public readonly string Repository;

    public readonly byte Flags;

    [SerializationConstructor]
    public PluginAlias(string repository, byte flags)
    {
        Repository = repository;
        Flags = flags;
    }
}

[Flags]
internal enum PluginAliasFlags : byte
{
    None = 0,
    Listed = 1,
    CanInstall = 1 << 1,
    All = Listed | CanInstall
}