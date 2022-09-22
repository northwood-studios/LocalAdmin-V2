using System;
using System.Linq;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Commands.PluginManager.Subcommands;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands.PluginManager;

internal class PluginManagerCommand : CommandBase
{
    public PluginManagerCommand() : base("p") { }

    internal override void Execute(string[] arguments)
    {
        if (arguments.Length == 0)
        {
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("---- Plugin Manager Commands ----", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("p check [-gl] [plugin name]- checks for updates.");
            ConsoleUtil.WriteLine("p install [-igo] <plugin name> [version] - lists all installed plugins.");
            ConsoleUtil.WriteLine("p list - lists all installed plugins.");
            ConsoleUtil.WriteLine("p remove [-ig] <plugin name> - lists all installed plugins.");
            ConsoleUtil.WriteLine("p update [-glo] - updates all installed plugins.");
            ConsoleUtil.WriteLine(string.Empty, ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("<required argument>, [optional argument] -g = global, -l = local, -o = overwrite", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("-i = ignore locks (don't use unless you know what you are doing)", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("plugin name = GitHub repository author and name, eg. author-name/repo-name", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If no version is specified then latest non-preview release is used.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If version is specified the plugin will be exempted from \"update\" command.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If both -g and -l arguments exist then by default (if unset) BOTH are checked.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("------------" + Environment.NewLine, ConsoleColor.DarkGray);
            return;
        }
        
        bool optionsSet = arguments.Length >= 2 && arguments[1].Length > 1 && arguments[1].StartsWith("-", StringComparison.Ordinal);
        string[]? args;
        var options = string.Empty;

        if (arguments.Length == 1)
            args = null;
        else if (optionsSet)
        {
            options = arguments[1][1..];
            args = arguments[2..];
        }
        else
            args = arguments[1..];
        
        switch (arguments[0].ToLowerInvariant())
        {
            case "check":
                break;
            
            case "install" when args == null || args.Length is 0 or > 2 || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') != 1:
                ConsoleUtil.WriteLine("Syntax error!", ConsoleColor.Red);
                break;
            
            case "install":
                InstallCommand.Install(args, options);
                break;
            
            case "list":
                break;
            
            case "remove":
                break;
            
            case "update":
                break;
            
            default:
                ConsoleUtil.WriteLine("Unknown command: " + arguments[0].ToLowerInvariant(), ConsoleColor.Red);
                break;
        }
    }
}