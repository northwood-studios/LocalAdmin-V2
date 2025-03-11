using System.Threading.Tasks;
using LocalAdmin.V2.Commands.Meta;

namespace LocalAdmin.V2.Commands;

internal sealed class RestartCommand() : CommandBase("Restart", "Restarts the server.")
{
    internal override ValueTask Execute(string[] arguments)
    {
        Core.LocalAdmin.Singleton.DisableExitActionSignals = true;
        Core.LocalAdmin.Singleton.ExitAction = Core.LocalAdmin.ShutdownAction.Restart;
        if (Core.LocalAdmin.Singleton.Server is { Connected: true })
            Core.LocalAdmin.Singleton.Server.WriteLine("exit");
        return ValueTask.CompletedTask;
    }
}