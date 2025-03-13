using System;
using System.Threading.Tasks;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands;

internal sealed class LaCfgCommand() : CommandBase("lacfg",
    "Prints the current LocalAdmin configuration and the configuration file path.")
{
    internal override ValueTask Execute(string[] arguments)
    {
        ConsoleUtil.WriteLine($"Current LocalAdmin config file path is: {Core.LocalAdmin.CurrentConfigPath ?? "(null)"}", ConsoleColor.DarkGreen);
        ConsoleUtil.WriteLine($"Current LocalAdmin Configuration:{Environment.NewLine}{Core.LocalAdmin.Configuration!}");
        return ValueTask.CompletedTask;
    }
}