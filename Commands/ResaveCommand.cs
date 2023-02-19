using System;
using System.IO;
using System.Text;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands;

internal sealed class ResaveCommand : CommandBase
{
    public ResaveCommand() : base("resave") { }

    internal override void Execute(string[] arguments)
    {
        try
        {
            if (Core.LocalAdmin.ConfigPath == null)
            {
                ConsoleUtil.WriteLine("Failed to resave config - ConfigPath is null.", ConsoleColor.Yellow);
                return;
            }

            if (Core.LocalAdmin.Configuration == null)
            {
                ConsoleUtil.WriteLine("Failed to resave config - Configuration is null.", ConsoleColor.Yellow);
                return;
            }

            File.WriteAllText(Core.LocalAdmin.ConfigPath, Core.LocalAdmin.Configuration.SerializeConfig(), Encoding.UTF8);
            ConsoleUtil.WriteLine("LocalAdmin config has been resaved!", ConsoleColor.DarkGreen);
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine("Failed to resave LocalAdmin config file!", ConsoleColor.Yellow);
            ConsoleUtil.WriteLine("Path: " + Core.LocalAdmin.ConfigPath, ConsoleColor.Yellow);
            ConsoleUtil.WriteLine("Exception: " + e.Message, ConsoleColor.Yellow);
        }
    }
}