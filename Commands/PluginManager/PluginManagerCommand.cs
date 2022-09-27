using System;
using System.Diagnostics;
using System.Linq;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Commands.PluginManager.Subcommands;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager;

internal class PluginManagerCommand : CommandBase
{
    public PluginManagerCommand() : base("p") { }
    
    private static Stopwatch? _securityWarningStopwatch;

    internal override async void Execute(string[] arguments)
    {
        if (!Core.LocalAdmin.DismissPluginsSecurityWarning && !Core.LocalAdmin.DataJson!.PluginManagerWarningDismissed)
        {
            if (_securityWarningStopwatch == null || arguments.Length != 1 || !arguments[0].Equals("confirm", StringComparison.OrdinalIgnoreCase))
            {
                _securityWarningStopwatch = new Stopwatch();
                _securityWarningStopwatch.Start();
                
                ConsoleUtil.WriteLine(string.Empty, ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("===== SECURITY WARNING =====", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("Plugin Manager is a tool designed to installing 3rd party modifications.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("These modifications are NOT developed by Northwood Studios (SCP:SL developers) and MAY contain malicious code that may harm the server.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("Northwood Studios and Local Admin developers take NO RESPONSIBILITY for any damage caused by any plugin.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("We recommend installing only well-known plugins from trusted sources.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine(string.Empty, ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("If you wish to continue, please type \"p confirm\" command.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("===== SECURITY WARNING =====", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine(string.Empty, ConsoleColor.Yellow);
                return;
            }
            
            if (_securityWarningStopwatch.ElapsedMilliseconds < 2000)
            {
                ConsoleUtil.WriteLine("Please take at least 2 seconds to read the security warning.", ConsoleColor.Yellow);
                return;
            }

            _securityWarningStopwatch = null;
            Core.LocalAdmin.DataJson.PluginManagerWarningDismissed = true;
            await Core.LocalAdmin.DataJson.TrySave(PathManager.InternalJsonDataPath);
        }
        
        if (arguments.Length == 0)
        {
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("---- Plugin Manager Commands ----", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("p check [-gl] [plugin name]- checks for plugins updates.");
            ConsoleUtil.WriteLine("p install [-igo] <plugin name> [version] - downloads and install a plugin.");
            ConsoleUtil.WriteLine("p list - lists all installed plugins.");
            ConsoleUtil.WriteLine("p remove [-ig] <plugin name> - uninstalls a plugin.");
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
            
            //Install
            case "i" when args == null || args.Length is 0 or > 2 || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') != 1:
            case "install" when args == null || args.Length is 0 or > 2 || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') != 1:
                
            //Remove
            case "remove" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') != 1:
            case "r" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') != 1:
            case "rm" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') != 1:
            case "uninstall" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') != 1:
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Syntax error!", ConsoleColor.Red);
                break;
            
            case "install":
            case "i":
                InstallCommand.Install(args, options);
                break;
            
            case "list":
                break;
            
            case "remove":
            case "r":
            case "rm":
            case "uninstall":
                await PluginInstaller.TryUninstallPlugin(args[0],
                    options.Contains('g', StringComparison.Ordinal) ? "global" : Core.LocalAdmin.GamePort.ToString(),
                    options.Contains('i', StringComparison.Ordinal));
                break;
            
            case "update":
                break;
            
            default:
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Unknown command: p " + arguments[0].ToLowerInvariant(), ConsoleColor.Red);
                break;
        }
    }
}