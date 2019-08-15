using System;

namespace LocalAdmin.V2.Commands
{
    public class HelpCommand : CommandBase
    {
        public HelpCommand() : base("Help") { }

        public override void Execute(string[] arguments)
        {
            ConsoleUtil.Write("");
            ConsoleUtil.Write("----HELP----", ConsoleColor.DarkGray);
            ConsoleUtil.Write("NEW - starts the next server on different port.");
            ConsoleUtil.Write("BAN - time-bans player using IP address or part of the nickname.");
            ConsoleUtil.Write("FORCESTART - forces the round to start.");
            ConsoleUtil.Write("ROUNDRESTART - forces the round to restart.");
            ConsoleUtil.Write("HELLO - tests if server is responding.");
            ConsoleUtil.Write("CONFIG - opens the server's configuration file.");
            ConsoleUtil.Write("CONFIG RELOAD - applies config changes.");
            ConsoleUtil.Write("EXIT - stops the server.");
            ConsoleUtil.Write("SEED - shows the current map seed in order to re-generate level in the future.");
            ConsoleUtil.Write("BANREFRESH - forces ban database to refresh.");
            ConsoleUtil.Write("------------" + Environment.NewLine, ConsoleColor.DarkGray);
        }
    }
}