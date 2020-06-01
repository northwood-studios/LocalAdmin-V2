using LocalAdmin.V2.Commands.Meta;

namespace LocalAdmin.V2.Commands
{
    internal sealed class RestartCommand : CommandBase
    {
        public RestartCommand() : base("Restart") { }

        internal override void Execute(string[] arguments)
        {
            Core.LocalAdmin.Singleton.StartSession(Core.LocalAdmin.Singleton.GamePort);
        }
    }
}
