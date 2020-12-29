using LocalAdmin.V2.Commands.Meta;

namespace LocalAdmin.V2.Commands
{
    internal sealed class RestartCommand : CommandBase
    {
        public RestartCommand() : base("Restart") { }

        internal override void Execute(string[] arguments)
        {
            Core.LocalAdmin.Singleton!.ExitAction = Core.LocalAdmin.ShutdownAction.Restart;
            if (Core.LocalAdmin.Singleton.Server != null && Core.LocalAdmin.Singleton.Server.Connected)
                Core.LocalAdmin.Singleton.Server.WriteLine("exit");
        }
    }
}
