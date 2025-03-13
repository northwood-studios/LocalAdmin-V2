using System;
using System.Collections.Generic;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace LocalAdmin.V2.JSON.Objects;

public record DataJson(
    string? GitHubPersonalAccessToken,
    DateTime? EulaAccepted,
    bool PluginManagerWarningDismissed,
    DateTime? LastPluginAliasesRefresh,
    Dictionary<string, PluginVersionCache> PluginVersionCache,
    Dictionary<string, PluginAlias> PluginAliases);

public readonly record struct PluginVersionCache(
    string Version,
    uint ReleaseId,
    DateTime PublishmentTime,
    DateTime LastRefreshed,
    string DllDownloadUrl,
    string? DependenciesDownloadUrl);

public readonly record struct PluginAlias(string Repository, byte Flags);

[Flags]
internal enum PluginAliasFlags : byte
{
    None = 0,
    Listed = 1,
    CanInstall = 1 << 1,
    All = Listed | CanInstall
}