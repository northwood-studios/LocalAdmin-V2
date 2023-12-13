namespace LocalAdmin.V2.Commands.Meta;

internal abstract class CommandBase
{
    public readonly string Name;
    public readonly string Description;
    public readonly bool SendToGame;

    protected CommandBase(string name, bool sendToGame = false)
    {
        Name = name.ToUpperInvariant();
        Description = "No Description Provided";
        SendToGame = sendToGame;
    }

    protected CommandBase(string name, string description, bool sendToGame = false)
    {
        Name = name.ToUpperInvariant();
        Description = description;
        SendToGame = sendToGame;
    }

    internal abstract void Execute(string[] arguments);
}