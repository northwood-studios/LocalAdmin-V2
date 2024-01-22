using System.Collections.Generic;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using TylerianPM.Models;
namespace TylerianPM;

internal sealed class PluginManagerCommand : CommandBase
{
    private List<Plugin> AvailablePlugins = new List<Plugin>();
    private List<Plugin> InstalledPlugins = new List<Plugin>();
    public PluginManagerCommand() : base("p", "LocalAdmin Plugin Manager.") { }
    internal override async void Execute(string[] arguments)
    {
        if (arguments.Length == 0)
        {
            Help();
            return;
        }

        switch (arguments[0].ToLower())
        {
            case "install":
                Install(arguments.Length >= 2 ? arguments[1] : null);
                return;
            case "update":
                Update(arguments.Length >= 2 ? arguments[1] : null);
                return;
            case "uninstall":
                Uninstall(arguments.Length >= 2 ? arguments[1] : null);
                return;
            case "about":
                ConsoleUtil.WriteLine("[PM]-> TylerianPM written by ALEXWARELLC & contributors.");
                return;
            case "help":
            default:
                Help();
                return;
        }
    }

    public async Task Install(string? ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            Help();
            return;
        }

        try
        {
            Plugin? plugin = await PluginManager.Instance?.CreatePlugin("https://store.alex.altexstudios.com/plugins.json");

            if (plugin.ID == ID)
                ConsoleUtil.WriteLine($"Retrieved Plugin: {plugin.Name} {plugin.ID}");
            else
                ConsoleUtil.WriteLine($"Unable to find a plugin which matched your query.");
        }
        catch (Exception ex)
        {
            ConsoleUtil.WriteLine($"[PM]-> An error occured while trying to perform an install operation. {ex}");
            return;
        }
    }

    public void Update(string? ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            Help();
            return;
        }
        ConsoleUtil.WriteLine($"[PM]-> Updating plugin '{ID}'...");
    }

    public void Uninstall(string? ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            Help();
            return;
        }
        ConsoleUtil.WriteLine($"[PM]-> Uninstalling plugin '{ID}'...");
    }

    public void Help()
    {
        ConsoleUtil.WriteLine("p install [ID] - Locates, downloads & installs the given plugin.");
        ConsoleUtil.WriteLine("p update [ID] - Updates an installed plugin.");
        ConsoleUtil.WriteLine("p uninstall [ID] - Uninstalls an installed plugin.");
    }
}