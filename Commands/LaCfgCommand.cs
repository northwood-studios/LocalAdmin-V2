using System;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands;

internal sealed class LaCfgCommand : CommandBase
{
    public LaCfgCommand() : base("lacfg") { }

    internal override void Execute(string[] arguments)
    {
        ConsoleUtil.WriteLine($"Current LocalAdmin config file path is: {Core.LocalAdmin.CurrentConfigPath ?? "(null)"}", ConsoleColor.DarkGreen);
        ConsoleUtil.WriteLine($"Current LocalAdmin Configuration:\n{LocalAdmin.V2.Core.LocalAdmin.Configuration.ToString()}");
    }
}