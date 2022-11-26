using System;
using System.Collections.Generic;
using Utf8Json;

namespace LocalAdmin.V2.PluginsManager;

public class ServerPluginsConfig
{
    public Dictionary<string, InstalledPlugin> InstalledPlugins;
    
    public Dictionary<string, Dependency> Dependencies;
    
    public DateTime? LastUpdateCheck;

    internal ServerPluginsConfig()
    {
        InstalledPlugins = new();
        Dependencies = new();
    }
    
    [SerializationConstructor]
    public ServerPluginsConfig(Dictionary<string, InstalledPlugin> installedPlugins, Dictionary<string, Dependency> dependencies, DateTime? lastUpdateCheck)
    {
        InstalledPlugins = installedPlugins;
        Dependencies = dependencies;
        LastUpdateCheck = lastUpdateCheck;
    }
}

public class InstalledPlugin
{
    public string? TargetVersion;
    
    public string? CurrentVersion;
    
    public string? FileHash;
    
    public DateTime InstallationDate;
    
    public DateTime UpdateDate;

    internal InstalledPlugin()
    {
        
    }
    
    [SerializationConstructor]
    public InstalledPlugin(string? targetVersion, string? currentVersion, string? fileHash, DateTime installationDate, DateTime updateDate)
    {
        TargetVersion = targetVersion;
        CurrentVersion = currentVersion;
        FileHash = fileHash;
        InstallationDate = installationDate;
        UpdateDate = updateDate;
    }
}

public class Dependency
{
    public string? FileHash;
    
    public DateTime InstallationDate;
    
    public DateTime UpdateDate;
    
    public bool ManuallyInstalled;
    
    public List<string> InstalledByPlugins;

    internal Dependency()
    {
        InstalledByPlugins = new();
    }
    
    [SerializationConstructor]
    public Dependency(string? fileHash, DateTime installationDate, DateTime updateDate, bool manuallyInstalled, List<string> installedByPlugins)
    {
        FileHash = fileHash;
        InstallationDate = installationDate;
        UpdateDate = updateDate;
        ManuallyInstalled = manuallyInstalled;
        InstalledByPlugins = installedByPlugins;
    }
}
