namespace LocalAdmin.V2.Commands.Meta;

internal abstract class CommandBase
{
    public readonly string Name;
    public readonly bool SendToGame;

    protected CommandBase(string name, bool sendToGame = false)
    {
        Name = name.ToUpperInvariant();
        SendToGame = sendToGame;
    }

    internal abstract void Execute(string[] arguments);
}