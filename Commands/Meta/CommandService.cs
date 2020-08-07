using System;
using System.Collections.Generic;

namespace LocalAdmin.V2.Commands.Meta
{
    internal class CommandService
    {
        private readonly Dictionary<string, CommandBase> commands = new Dictionary<string, CommandBase>(StringComparer.OrdinalIgnoreCase);

        internal void RegisterCommand(CommandBase command)
        {
            commands.Add(command.Name, command);
        }

        internal void UnregisterCommand(CommandBase command)
        {
            if (!commands.Remove(command.Name))
                throw new KeyNotFoundException();
        }

        internal CommandBase? GetCommandByName(string name)
        {
            return commands.TryGetValue(name, out CommandBase? command) ? command : null;
        }
    }
}