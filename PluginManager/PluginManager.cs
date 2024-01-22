using LocalAdmin.V2.IO;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TylerianPM.Models;
using Utf8Json;

namespace TylerianPM;

internal class PluginManager
{
    public static PluginManager? Instance { get; private set; }
    private readonly ushort _port;
    public PluginManager(ushort Port)
    {
        Instance = this;
        _port = Port;
    }

    internal string PluginsPath => $"{PathManager.GameUserDataRoot}PluginAPI{Path.DirectorySeparatorChar}plugins{Path.DirectorySeparatorChar}{_port}{Path.DirectorySeparatorChar}";
    private string DependenciesPath => $"{PluginsPath}dependencies{Path.DirectorySeparatorChar}";
    private string TempPath => $"{PathManager.GameUserDataRoot}internal{Path.DirectorySeparatorChar}LA Temp{Path.DirectorySeparatorChar}{_port}{Path.DirectorySeparatorChar}";

    public async Task<Plugin?> CreatePlugin(string Source)
    {
        try
        {
            return JsonSerializer.Deserialize<Plugin>(await GetString(Source));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<string> GetString(string Location)
    {
        HttpClient _client = new();

        return await _client.GetStringAsync(Location) ?? string.Empty;
    }
}
