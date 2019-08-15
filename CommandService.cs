using System.Collections.Generic;

namespace LocalAdmin.V2
{
    public class CommandService
    {
        private readonly List<CommandBase> commands = new List<CommandBase>();

        public void RegisterCommand(CommandBase command)
        {
            commands.Add(command);
        }

        public void UnregisterCommand(CommandBase command)
        {
            if (commands.Contains(command))
                commands.Remove(command);
            else
                throw new KeyNotFoundException();
        }

        public CommandBase GetCommandByName(string name)
        {
            foreach (var command in commands)
                if (command.Name == name.ToUpper())
                    return command;

            return null;
        }
    }
}