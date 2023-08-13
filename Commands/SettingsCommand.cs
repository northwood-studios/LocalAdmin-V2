using LocalAdmin.V2.IO;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Core;
using System;

namespace LocalAdmin.V2.Commands;

internal sealed class SettingsCommand : CommandBase
{
    Config Settings;
    public SettingsCommand() : base("settings") { }
    internal override void Execute(string[] arguments)
    {
        Settings = LocalAdmin.V2.Core.LocalAdmin.Configuration;
        ConsoleUtil.WriteLine(" |=====| Settings |=====| ");
        Console.WriteLine();
        GetConfigSetting();
    }
    private void GetConfigSetting()
    {
        ConsoleUtil.Write("RestartOnCrash: ");
        if (Settings.RestartOnCrash)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        // -----

        ConsoleUtil.Write("EnableHeartbeat: ");
        if (Settings.EnableHeartbeat)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaLiveViewUseUtc: ");
        if (Settings.LaLiveViewUseUtc)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaShowStdoutStderr: ");
        if (Settings.LaShowStdoutStderr)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaNoSetCursor: ");
        if (Settings.LaNoSetCursor)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("EnableTrueColor: ");
        if (Settings.EnableTrueColor)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("EnableLaLogs: ");
        if (Settings.EnableLaLogs)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaLogsUseUtc: ");
        if (Settings.LaLogsUseUtc)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaLogsUseZForUtc: ");
        if (Settings.LaLogsUseZForUtc)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaLogAutoFlush: ");
        if (Settings.LaLogAutoFlush)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaLogStdoutStderr: ");
        if (Settings.LaLogStdoutStderr)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaDeleteOldLogs: ");
        if (Settings.LaDeleteOldLogs)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("true");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("false");
            Console.ForegroundColor = ConsoleColor.White;
        }

        ConsoleUtil.Write("LaLogsExpirationDays: ");
        if (Settings.LaLogsExpirationDays > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Settings.LaLogsExpirationDays);
            Console.ForegroundColor = ConsoleColor.White;

            //-----------

            ConsoleUtil.Write("DeleteOldRoundLogs: ");
            if (Settings.DeleteOldRoundLogs)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("true");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("false");
                Console.ForegroundColor = ConsoleColor.White;
            }

            ConsoleUtil.Write("RoundLogsExpirationDays: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Settings.RoundLogsExpirationDays);
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtil.Write("CompressOldRoundLogs: ");
            if (Settings.CompressOldRoundLogs)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("true");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("false");
                Console.ForegroundColor = ConsoleColor.White;
            }

            ConsoleUtil.Write("LaLogsExpirationDays: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Settings.RoundLogsCompressionThresholdDays);
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtil.Write("HeartbeatSpanMaxThreshold: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Settings.HeartbeatSpanMaxThreshold);
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtil.Write("LaToSlBufferSize: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Settings.LaToSlBufferSize);
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtil.Write("SlToLaBufferSize: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Settings.SlToLaBufferSize);
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtil.Write("LaLiveViewTimeFormat: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Settings.LaLiveViewTimeFormat);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
