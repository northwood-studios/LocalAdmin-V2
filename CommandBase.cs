using System.Globalization;

namespace LocalAdmin.V2
{
    internal abstract class CommandBase
    {
        public readonly string Name;

        protected CommandBase(string name) => Name = name.ToUpper(CultureInfo.InvariantCulture);

        internal abstract void Execute(string[] arguments);
    }
}