using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using System;

namespace LocalAdmin.V2.Commands;

internal sealed class HelpCommand : CommandBase
{
    public HelpCommand() : base("Help", true) { }

    internal override void Execute(string[] arguments)
    {
        ConsoleUtil.WriteLine(string.Empty);
        ConsoleUtil.WriteLine("---- LocalAdmin Commands ----", ConsoleColor.DarkGray);
        ConsoleUtil.WriteLine("EXIT - stops the server.");
        ConsoleUtil.WriteLine("FORCERESTART - kills the server and restarts it.");
        ConsoleUtil.WriteLine("HBC - cancels heartbeat restart countdown.");
        ConsoleUtil.WriteLine("HBCTRL - controls heartbeat.");
        ConsoleUtil.WriteLine("HELP - prints this help.");
        ConsoleUtil.WriteLine("LACFG - prints the current LocalAdmin configuration and the config file path.");
        ConsoleUtil.WriteLine("LICENSE - prints LocalAdmin license details.");
        ConsoleUtil.WriteLine("P - Plugin Manager.");
        ConsoleUtil.WriteLine("RESAVE - resaves the LocalAdmin config file.");
        ConsoleUtil.WriteLine("RESTART - restarts the server.");
        ConsoleUtil.WriteLine("------------" + Environment.NewLine, ConsoleColor.DarkGray);
        ConsoleUtil.WriteLine("---- Game Commands Commands ----", ConsoleColor.DarkGray);
    }
}