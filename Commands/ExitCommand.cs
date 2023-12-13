using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Core;

namespace LocalAdmin.V2.Commands;

internal sealed class ExitCommand : CommandBase
{
    public ExitCommand() : base("Exit", "Stops the server.", true) { }

    internal override void Execute(string[] arguments) { }
}