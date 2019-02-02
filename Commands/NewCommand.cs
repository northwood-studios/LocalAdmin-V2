using System;
using System.Diagnostics;

namespace LocalAdmin_V2_Net_Core.Commands
{
    public class NewCommand : CommandBase
    {
        public NewCommand() : base("New")
        {
        }

        public override void Execute(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                var port = -1;
                if (int.TryParse(arguments[0], out port))
                    Process.Start(new ProcessStartInfo(Program.localAdminExecutable, Convert.ToString(port)));
                else
                    ConsoleUtil.Write("Usage: new port", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleUtil.Write("Usage: new port", ConsoleColor.Yellow);
            }
        }
    }
}