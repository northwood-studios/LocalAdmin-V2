using System;
using LocalAdmin.V2.Commands.Meta;
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
            ConsoleUtil.WriteLine("p cupdate [-gl] [plugin name]- checks for updates.");
            ConsoleUtil.WriteLine("p install [-go] <plugin name> [version] - lists all installed plugins.");
            ConsoleUtil.WriteLine("p list - lists all installed plugins.");
            ConsoleUtil.WriteLine("p remove [-g] <plugin name> - lists all installed plugins.");
            ConsoleUtil.WriteLine("p update [-glo] - updates all installed plugins.");
            ConsoleUtil.WriteLine(string.Empty, ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("<required argument>, [optional argument] -g = global, -l = local, -o = overwrite", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("plugin name = GitHub repository author and name, eg. author-name/repo-name", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If no version is specified then latest non-preview release is used.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If version is specified the plugin will be exempted from \"update\" command.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If both -g and -l arguments exist then by default (if unset) BOTH are checked.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("------------" + Environment.NewLine, ConsoleColor.DarkGray);
            return;
        }
    }
}