using System;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands;

internal sealed class LaCfgCommand : CommandBase
{
    public LaCfgCommand() : base("lacfg", "Prints the current LocalAdmin configuration and the configuration file path.") { }

    internal override void Execute(string[] arguments)
    {
        ConsoleUtil.WriteLine($"Current LocalAdmin config file path is: {Core.LocalAdmin.CurrentConfigPath ?? "(null)"}", ConsoleColor.DarkGreen);
        ConsoleUtil.WriteLine($"Current LocalAdmin Configuration:{Environment.NewLine}{LocalAdmin.V2.Core.LocalAdmin.Configuration!.ToString()}");
    }
}