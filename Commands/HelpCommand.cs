using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalAdmin.V2.Commands;

internal sealed class HelpCommand : CommandBase
{
    public HelpCommand() : base("Help", true) { }

    internal override void Execute(string[] arguments)
    {
        var commands = from p in CommandService.GetAllCommands() orderby p.Name ascending select p;

        ConsoleUtil.WriteLine(string.Empty);
        ConsoleUtil.WriteLine("---- LocalAdmin Commands ----", ConsoleColor.DarkGray);
        ConsoleUtil.WriteLine("EXIT - stops the server.");

        foreach (var item in commands)
        {
            ConsoleUtil.WriteLine($"{item.Name} - {item.Description}");
        }

        ConsoleUtil.WriteLine("---- Game Commands ----", ConsoleColor.DarkGray);
    }
}