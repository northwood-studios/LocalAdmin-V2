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
                ConsoleUtil.WriteLine("Northwood Studios and LocalAdmin developers take NO RESPONSIBILITY for any damage caused by any plugin.", ConsoleColor.Yellow);
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
            ConsoleUtil.WriteLine("Plugin manager has been enabled. USE AT YOUR OWN RISK.", ConsoleColor.Yellow);
            Core.LocalAdmin.DataJson.PluginManagerWarningDismissed = true;
            await Core.LocalAdmin.DataJson.TrySave(PathManager.InternalJsonDataPath);
            return;
        }
        
        if (arguments.Length == 0)
        {
            ConsoleUtil.WriteLine(string.Empty);
            ConsoleUtil.WriteLine("---- Plugin Manager Commands ----", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("p check [-igl] - checks for plugins updates.");
            ConsoleUtil.WriteLine("p install [-igos] <plugin name> [version] - downloads and installs a plugin.");
            ConsoleUtil.WriteLine("p list [-igls] - lists all installed plugins.");
            ConsoleUtil.WriteLine("p maintenance [-igl] - runs a plugins maintenance.");
            ConsoleUtil.WriteLine("p refresh - refreshes list of plugin aliases.");
            ConsoleUtil.WriteLine("p remove [-igs] <plugin name> - uninstalls a plugin.");
            ConsoleUtil.WriteLine("p token [GitHub PAT] - sets, clears or provides more info about GitHub Personal Authentication Token.");
            ConsoleUtil.WriteLine("p update [-iglos] - updates all installed plugins.");
            ConsoleUtil.WriteLine(string.Empty, ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("<required argument>, [optional argument] -g = global (all ports), -l = local (current port), -o = overwrite", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("-i = ignore locks (don't use unless you know what you are doing)", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("-s = skip checking for updates and refreshing list (don't use unless you know what you are doing)", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("plugin name = GitHub repository author and name, eg. author-name/repo-name", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If no version is specified then latest non-preview release is used.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If version is specified the plugin will be exempted from \"update\" command.", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("If both -g and -l arguments exist then by default (if unset) BOTH are used.", ConsoleColor.DarkGray);
            
            if (string.IsNullOrEmpty(Core.LocalAdmin.DataJson!.GitHubPersonalAccessToken))
            {
                ConsoleUtil.WriteLine(string.Empty, ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine("GitHub Personal Access Token is not set in LocalAdmin.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("You may exceed GitHub rate limits if you use Plugin Manager extensively.", ConsoleColor.Yellow);
                ConsoleUtil.WriteLine("Run \"p token\" command to get more details.", ConsoleColor.Yellow);
            }
            
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
            case "c":
            case "ch":
            case "chk":
                CheckCommand.Check(options);
                break;
            
            //Install
            case "i" when args == null || args.Length is 0 or > 2 || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') > 1:
            case "install" when args == null || args.Length is 0 or > 2 || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') > 1:
                
            //Remove
            case "remove" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') > 1:
            case "r" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') > 1:
            case "rm" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') > 1:
            case "uninstall" when args is not { Length: 1 } || string.IsNullOrEmpty(args[0]) || args[0].Count(x => x == '/') > 1:
                
            //token
            case "token" when args is { Length: > 1 }:
            case "t" when args is { Length: > 1 }:
            case "pat" when args is { Length: > 1 }:
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Syntax error!", ConsoleColor.Red);
                break;
            
            case "install":
            case "i":
                InstallCommand.Install(args, options);
                break;
            
            case "list":
            case "l":
            case "ls":
                ListCommand.List(options);
                break;
            
            case "maintenance":
            case "m":
            case "mn":
            case "mnt":
                MaintenanceCommand.Maintenance(options);
                break;
            
            case "refresh":
            case "ref":
            case "rf":
                _ = OfficialPluginsList.RefreshOfficialPluginsList();
                break;
            
            case "remove":
            case "r":
            case "rm":
            case "uninstall":
                _ = PluginInstaller.TryUninstallPlugin(args[0],
                    options.Contains('g', StringComparison.Ordinal) ? "global" : Core.LocalAdmin.GamePort.ToString(),
                    options.Contains('i', StringComparison.Ordinal),
                    options.Contains('s', StringComparison.Ordinal));
                break;
            
            case "token":
            case "t":
            case "pat":
                TokenCommand.Token(args == null || args.Length == 0 ? null : args[0]);
                break;
            
            case "update":
            case "u":
            case "up":
            case "upd":
                UpdateCommand.Update(options);
                break;
            
            default:
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] Unknown command: p " + arguments[0].ToLowerInvariant(), ConsoleColor.Red);
                break;
        }
    }
}