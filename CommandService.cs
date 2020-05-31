using System.Collections.Generic;
using System.Globalization;

namespace LocalAdmin.V2
{
    internal class CommandService
    {
        private readonly List<CommandBase> commands = new List<CommandBase>();

        internal void RegisterCommand(CommandBase command)
        {
            commands.Add(command);
        }

        internal void UnregisterCommand(CommandBase command)
        {
            if (commands.Contains(command))
                commands.Remove(command);
            else
                throw new KeyNotFoundException();
        }

        internal CommandBase? GetCommandByName(string name)
        {
            foreach (var command in commands)
                if (command.Name == name.ToUpper(CultureInfo.InvariantCulture))
                    return command;

            return null;
        }
    }
}