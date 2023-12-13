using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using System;
using System.Linq;

namespace LocalAdmin.V2.Commands;

internal sealed class HelpCommand : CommandBase
{
    public HelpCommand() : base("Help", "Prints all available commands.", true) { }

    internal override void Execute(string[] arguments)
    {
        var commands = Core.LocalAdmin.Singleton?.CommandService.GetAllCommands().OrderBy(p => p.Name);

        ConsoleUtil.WriteLine(string.Empty);
        ConsoleUtil.WriteLine("---- LocalAdmin Commands ----", ConsoleColor.DarkGray);

        if (commands is not null)
        {
            foreach (var item in commands)
            {
                ConsoleUtil.WriteLine($"{item.Name} - {item.Description}");
            }
        }
        
        ConsoleUtil.WriteLine("------------" + Environment.NewLine, ConsoleColor.DarkGray);
        ConsoleUtil.WriteLine("---- Game Commands ----", ConsoleColor.DarkGray);
    }
}