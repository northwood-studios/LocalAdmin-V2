using System.Threading.Tasks;
using LocalAdmin.V2.Commands.Meta;

namespace LocalAdmin.V2.Commands;

internal sealed class ExitCommand() : CommandBase("Exit", "Stops the server.", true)
{
    internal override ValueTask Execute(string[] arguments) => ValueTask.CompletedTask;
}