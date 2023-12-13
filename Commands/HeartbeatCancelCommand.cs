using System;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands;

internal sealed class HeartbeatCancelCommand : CommandBase
{
    public HeartbeatCancelCommand() : base("hbc", "Cancels heartbeat restart countdown.") { }

    internal override void Execute(string[] arguments)
    {
        if (Core.LocalAdmin.Singleton!.CurrentHeartbeatStatus != Core.LocalAdmin.HeartbeatStatus.Active)
        {
            ConsoleUtil.WriteLine("Heartbeat is not active!", ConsoleColor.Yellow);
            return;
        }

        if (Core.LocalAdmin.Singleton.HeartbeatWarningStage == 0)
        {
            ConsoleUtil.WriteLine("Heartbeat restart countdown has not started!", ConsoleColor.Yellow);
            return;
        }

        Core.LocalAdmin.Singleton.CurrentHeartbeatStatus = Core.LocalAdmin.HeartbeatStatus.AwaitingFirstHeartbeat;

        ConsoleUtil.WriteLine("Heartbeat restart countdown has been cancelled.", ConsoleColor.DarkGreen);
        ConsoleUtil.WriteLine("Crash detection will be resumed after receiving any heartbeat. If you want to disable heartbeat completely for this LocalAdmin session (until server is restarted) run \"hbctrl 0\" command.", ConsoleColor.DarkGreen);
    }
}