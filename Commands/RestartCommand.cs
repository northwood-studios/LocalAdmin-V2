using LocalAdmin.V2.Commands.Meta;

namespace LocalAdmin.V2.Commands;

internal sealed class RestartCommand : CommandBase
{
    public RestartCommand() : base("Restart", "Restarts the server.") { }

    internal override void Execute(string[] arguments)
    {
        Core.LocalAdmin.Singleton!.DisableExitActionSignals = true;
        Core.LocalAdmin.Singleton.ExitAction = Core.LocalAdmin.ShutdownAction.Restart;
        if (Core.LocalAdmin.Singleton.Server is { Connected: true })
            Core.LocalAdmin.Singleton.Server.WriteLine("exit");
    }
}