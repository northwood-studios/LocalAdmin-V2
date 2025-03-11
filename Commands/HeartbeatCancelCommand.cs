using System;
using System.Threading.Tasks;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands;

internal sealed class HeartbeatCancelCommand() : CommandBase("hbc", "Cancels heartbeat restart countdown.")
{
    internal override ValueTask Execute(string[] arguments)
    {
        if (Core.LocalAdmin.Singleton.CurrentHeartbeatStatus != Core.LocalAdmin.HeartbeatStatus.Active)
        {
            ConsoleUtil.WriteLine("Heartbeat is not active!", ConsoleColor.Yellow);
            return ValueTask.CompletedTask;
        }

        if (Core.LocalAdmin.Singleton.HeartbeatWarningStage == 0)
        {
            ConsoleUtil.WriteLine("Heartbeat restart countdown has not started!", ConsoleColor.Yellow);
            return ValueTask.CompletedTask;
        }

        Core.LocalAdmin.Singleton.CurrentHeartbeatStatus = Core.LocalAdmin.HeartbeatStatus.AwaitingFirstHeartbeat;

        ConsoleUtil.WriteLine("Heartbeat restart countdown has been cancelled.", ConsoleColor.DarkGreen);
        ConsoleUtil.WriteLine("Crash detection will be resumed after receiving any heartbeat. If you want to disable heartbeat completely for this LocalAdmin session (until server is restarted) run \"hbctrl 0\" command.", ConsoleColor.DarkGreen);
        return ValueTask.CompletedTask;
    }
}