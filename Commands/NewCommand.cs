using System;
using System.Diagnostics;

namespace LocalAdmin.V2.Commands
{
    internal class NewCommand : CommandBase
    {
        public NewCommand() : base("New") { }

        internal override void Execute(string[] arguments)
        {
            ushort port = 7777;

            if (arguments.Length == 1)
            {
                if (ushort.TryParse(arguments[0], out port))
                    StartNew(port);
                else
                    ConsoleUtil.WriteLine("Usage: new port", ConsoleColor.Yellow);
            }
            else
                ConsoleUtil.WriteLine("Usage: new port", ConsoleColor.Yellow);
        }

        private void StartNew(ushort port)
        {
            Process.Start(new ProcessStartInfo(Program.localAdmin!.LocalAdminExecutable!, Convert.ToString(port)));
            Program.localAdmin!.Exit(1); // Terminate the previous session
        }
    }
}