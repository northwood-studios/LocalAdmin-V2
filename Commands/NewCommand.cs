using System;
using System.Diagnostics;

namespace LocalAdmin.V2.Commands
{
    public class NewCommand : CommandBase
    {
        private string _executable;

        public NewCommand(string executable) : base("New")
        {
            _executable = executable;
        }

        public override void Execute(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                var port = -1;
                if (int.TryParse(arguments[0], out port))
                    Process.Start(new ProcessStartInfo(_executable, Convert.ToString(port)));
                else
                    ConsoleUtil.WriteLine("Usage: new port", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleUtil.WriteLine("Usage: new port", ConsoleColor.Yellow);
            }
        }
    }
}