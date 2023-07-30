using System;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Commands;

internal sealed class HeartbeatControlCommand : CommandBase
{
    public HeartbeatControlCommand() : base("hbctrl") { }

    internal override void Execute(string[] arguments)
    {
        if (!Core.LocalAdmin.Singleton!.EnableGameHeartbeat)
        {
            ConsoleUtil.WriteLine("Heartbeat is not enabled in the LA config!", ConsoleColor.Yellow);
            return;
        }

        if (arguments.Length != 1)
        {
            ConsoleUtil.WriteLine("Usage: hbctrl <enable|disable|status>", ConsoleColor.Yellow);
            return;
        }

        switch (arguments[0].ToLowerInvariant())
        {
            case "enable" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus == Core.LocalAdmin.HeartbeatStatus.Disabled:
            case "en" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus == Core.LocalAdmin.HeartbeatStatus.Disabled:
            case "e" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus == Core.LocalAdmin.HeartbeatStatus.Disabled:
            case "1" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus == Core.LocalAdmin.HeartbeatStatus.Disabled:
                Core.LocalAdmin.Singleton.CurrentHeartbeatStatus =
                    Core.LocalAdmin.HeartbeatStatus.AwaitingFirstHeartbeat;

                ConsoleUtil.WriteLine("Heartbeat has been enabled.", ConsoleColor.DarkGreen);
                break;

            case "enable":
            case "en":
            case "e":
            case "1":
                ConsoleUtil.WriteLine("Heartbeat is already enabled.", ConsoleColor.Gray);
                break;

            case "disable" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus != Core.LocalAdmin.HeartbeatStatus.Disabled:
            case "dis" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus != Core.LocalAdmin.HeartbeatStatus.Disabled:
            case "d" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus != Core.LocalAdmin.HeartbeatStatus.Disabled:
            case "0" when Core.LocalAdmin.Singleton.CurrentHeartbeatStatus != Core.LocalAdmin.HeartbeatStatus.Disabled:
                Core.LocalAdmin.Singleton.CurrentHeartbeatStatus =
                    Core.LocalAdmin.HeartbeatStatus.Disabled;

                ConsoleUtil.WriteLine("Heartbeat has been disabled.", ConsoleColor.DarkGreen);
                break;

            case "disable":
            case "dis":
            case "d":
            case "0":
                ConsoleUtil.WriteLine("Heartbeat is already disabled.", ConsoleColor.Gray);
                break;

            case "status":
            case "st":
            case "s":
                ConsoleUtil.WriteLine($"Heartbeat status: {HeartbeatStatusString}\nHeartbeat restart stage: {Core.LocalAdmin.Singleton.HeartbeatWarningStage}\nLast heartbeat was received {Core.LocalAdmin.HeartbeatStopwatch.ElapsedMilliseconds} ms ago.", ConsoleColor.DarkGreen);
                break;

            default:
                ConsoleUtil.WriteLine("Unknown subcommand. Run \"hbctrl\" for get command usage.", ConsoleColor.Red);
                break;
        }
    }

    private static string HeartbeatStatusString
    {
        get
        {
            switch (Core.LocalAdmin.Singleton!.CurrentHeartbeatStatus)
            {
                case Core.LocalAdmin.HeartbeatStatus.Disabled:
                    return "DISABLED";

                case Core.LocalAdmin.HeartbeatStatus.AwaitingFirstHeartbeat:
                    return "ACTIVE - AWAITING FIRST HEARTBEAT";

                case Core.LocalAdmin.HeartbeatStatus.Active:
                    return "ACTIVE - MONITORING";

                default:
                    return "(unknown)";
            }
        }
    }
}