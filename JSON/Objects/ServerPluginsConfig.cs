using System;
using System.Collections.Generic;

namespace LocalAdmin.V2.JSON.Objects;

public record ServerPluginsConfig(
    Dictionary<string, InstalledPlugin> InstalledPlugins,
    Dictionary<string, Dependency> Dependencies,
    DateTime? LastUpdateCheck);

public record InstalledPlugin(
    string? TargetVersion,
    string? CurrentVersion,
    string? FileHash,
    DateTime InstallationDate,
    DateTime UpdateDate);

public record Dependency(
    string? FileHash,
    DateTime InstallationDate,
    DateTime UpdateDate,
    bool ManuallyInstalled,
    List<string> InstalledByPlugins);