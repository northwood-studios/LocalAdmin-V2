using System;

namespace LocalAdmin.V2.Commands
{
    public class HelpCommand : CommandBase
    {
        public HelpCommand() : base("Help") { }

        public override void Execute(string[] arguments)
        {
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("----HELP----", ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("NEW - starts the next server on different port.");
            ConsoleUtil.WriteLine("BAN - time-bans player using IP address or part of the nickname.");
            ConsoleUtil.WriteLine("FORCESTART - forces the round to start.");
            ConsoleUtil.WriteLine("ROUNDRESTART - forces the round to restart.");
            ConsoleUtil.WriteLine("HELLO - tests if server is responding.");
            ConsoleUtil.WriteLine("CONFIG - opens the server's configuration file.");
            ConsoleUtil.WriteLine("CONFIG RELOAD - applies config changes.");
            ConsoleUtil.WriteLine("EXIT - stops the server.");
            ConsoleUtil.WriteLine("SEED - shows the current map seed in order to re-generate level in the future.");
            ConsoleUtil.WriteLine("BANREFRESH - forces ban database to refresh.");
            ConsoleUtil.WriteLine("------------" + Environment.NewLine, ConsoleColor.DarkGray);
        }
    }
}