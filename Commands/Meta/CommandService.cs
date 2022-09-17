using System;
using System.Collections.Generic;

namespace LocalAdmin.V2.Commands.Meta
{
    internal class CommandService
    {
        private readonly Dictionary<string, CommandBase> _commands = new(StringComparer.OrdinalIgnoreCase);

        internal void RegisterCommand(CommandBase command)
        {
            _commands.Add(command.Name, command);
        }

        internal void UnregisterCommand(CommandBase command)
        {
            if (!_commands.Remove(command.Name))
                throw new KeyNotFoundException();
        }

        internal CommandBase? GetCommandByName(string name)
        {
            return _commands.TryGetValue(name, out CommandBase? command) ? command : null;
        }
    }
}