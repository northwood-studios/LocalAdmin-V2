using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Core;
using LocalAdmin.V2.IO;
using System;
using System.Diagnostics;

namespace LocalAdmin.V2.Commands
{
    internal class NewCommand : CommandBase
    {
        public NewCommand() : base("New") { }

        internal override void Execute(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                if (ushort.TryParse(arguments[0], out var port))
                    StartNew(port);
                else
                    ConsoleUtil.WriteLine("Usage: new port", ConsoleColor.Yellow);
            }
            else
                ConsoleUtil.WriteLine("Usage: new port", ConsoleColor.Yellow);
        }

        private void StartNew(ushort port)
        {
            var processStartInfo = new ProcessStartInfo(Program.localAdmin!.LocalAdminExecutable!, Convert.ToString(port));
            // Use a new shell to launch
            // Actual for Windows Terminal - when launched inside an existing console, it breaks
            processStartInfo.UseShellExecute = true;
            Process.Start(processStartInfo);
            Program.localAdmin!.Exit(1); // Terminate the previous session
        }
    }
}