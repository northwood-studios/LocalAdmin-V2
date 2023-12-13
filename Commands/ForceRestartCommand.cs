using LocalAdmin.V2.Commands.Meta;

namespace LocalAdmin.V2.Commands;

internal sealed class ForceRestartCommand : CommandBase
{
    public ForceRestartCommand() : base("Forcerestart", "Kills and restarts the server.") { }

    internal override void Execute(string[] arguments)
    {
        Core.LocalAdmin.Singleton!.ExitAction = Core.LocalAdmin.ShutdownAction.Restart;
        Core.LocalAdmin.Singleton.Exit(0, restart: true);
    }
}