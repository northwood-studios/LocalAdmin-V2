using System.Threading.Tasks;

namespace LocalAdmin.V2.Commands.Meta;

internal abstract class CommandBase(string name, string description, bool sendToGame = false)
{
    public readonly string Name = name;
    public readonly string Description = description;
    public readonly bool SendToGame = sendToGame;

    protected CommandBase(string name, bool sendToGame = false) : this(name, "No Description Provided", sendToGame)
    {
    }

    internal abstract ValueTask Execute(string[] arguments);
}