using LocalAdmin.V2.IO;
using System.Collections.Generic;

namespace LocalAdmin.V2.PluginsManager;

/// <summary>
/// LabAPI configuration used by Plugins Manager.
/// </summary>
public class LabApiConfig
{
    /// <summary>
    /// List of dependency paths relative to <see cref="PathManager.DependenciesPath"/>.
    /// </summary>
    public List<string> DependencyPaths { get; set; } = new() { "global", PluginContext.PortFolder };

    /// <summary>
    /// List of plugin paths relative to <see cref="PathManager.PluginsPath"/>.
    /// </summary>
    public List<string> PluginPaths { get; set; } = new() { "global", PluginContext.PortFolder };
}