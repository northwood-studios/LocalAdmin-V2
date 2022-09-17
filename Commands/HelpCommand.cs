using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using System;

namespace LocalAdmin.V2.Commands;

internal class HelpCommand : CommandBase
{
    public HelpCommand() : base("Help", true) { }

    internal override void Execute(string[] arguments)
    {
        ConsoleUtil.WriteLine(string.Empty);
        ConsoleUtil.WriteLine("---- Local Admin Commands ----", ConsoleColor.DarkGray);
        ConsoleUtil.WriteLine("EXIT - stops the server.");
        ConsoleUtil.WriteLine("FORCERESTART - kills the server and restarts it.");
        ConsoleUtil.WriteLine("HELP - prints this help.");
        ConsoleUtil.WriteLine("LICENSE - prints Local Admin license details.");
        ConsoleUtil.WriteLine("P - Plugin Manager.");
        ConsoleUtil.WriteLine("RESTART - restarts the server.");
        ConsoleUtil.WriteLine("------------" + Environment.NewLine, ConsoleColor.DarkGray);
        ConsoleUtil.WriteLine("---- Game Commands Commands ----", ConsoleColor.DarkGray);
    }
}