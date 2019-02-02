namespace LocalAdmin_V2_Net_Core
{
    public abstract class CommandBase
    {
        public readonly string Name;

        public CommandBase(string name)
        {
            Name = name.ToUpper();
        }

        public abstract void Execute(string[] arguments);
    }
}