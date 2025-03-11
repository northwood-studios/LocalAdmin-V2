using System.Threading.Tasks;
using LocalAdmin.V2.Commands.Meta;

namespace LocalAdmin.V2.Commands;

internal sealed class ForceRestartCommand() : CommandBase("Forcerestart", "Kills and restarts the server.")
{
    internal override ValueTask Execute(string[] arguments)
    {
        Core.LocalAdmin.Singleton.ExitAction = Core.LocalAdmin.ShutdownAction.Restart;
        Core.LocalAdmin.Singleton.Exit(0, restart: true);
        return ValueTask.CompletedTask;
    }
}