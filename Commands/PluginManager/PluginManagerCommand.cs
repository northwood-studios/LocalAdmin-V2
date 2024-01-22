using System.Collections.Generic;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using System.Linq;
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
                ConsoleUtil.WriteLine("TylerianPM written by ALEXWARELLC & contributors.");
                return;


            case "help":
            default:
                Help();
                return;
        }
    }

    public void Install(string? ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            Help();
            return;
        }
        ConsoleUtil.WriteLine($"Attmpting to install plugin '{ID}'...");
    }

    public void Update(string? ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            Help();
            return;
        }
        ConsoleUtil.WriteLine($"Updating plugin '{ID}'...");
    }

    public void Uninstall(string? ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            Help();
            return;
        }
        ConsoleUtil.WriteLine($"Uninstalling plugin '{ID}'...");
    }

    public void Help()
    {
        ConsoleUtil.WriteLine("p install [ID] - Locates, downloads & installs the given plugin.");
        ConsoleUtil.WriteLine("p update [ID] - Updates an installed plugin.");
        ConsoleUtil.WriteLine("p uninstall [ID] - Uninstalls an installed plugin.");
    }
}