using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Core;

namespace LocalAdmin.V2.Commands
{
    internal sealed class RestartCommand : CommandBase
    {
        public RestartCommand() : base("Restart") { }

        internal override void Execute(string[] arguments)
        {
            Program.localAdmin!.StartSession(Program.localAdmin.GamePort);
        }
    }
}
