using System;
using System.Diagnostics;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Core;

class SteamIntegrityCheck
{
    //TODO: Add Non-Steam way of verifying LocalAdmin files.
    //IMPORTANT: `VerifyBySteam` uses the currently installed version from Steam. Builds that aren't installed via Steam cannot be validated.
    public static void RunAppValidation(bool VerifyBySteam)
    {
        if (!VerifyBySteam)
        {
            ConsoleUtil.WriteLine("Validation is not currently supported on devices without Steam installed.", ConsoleColor.Red);
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            if (VerifyBySteam)
            {
                Console.WriteLine("Verifying LocalAdmin using Steam (Windows)...", ConsoleColor.DarkGreen);
                Process.Start(new ProcessStartInfo("steam://validate/996560") { UseShellExecute = true });
                return;
            }
        }
        if (OperatingSystem.IsLinux())
        {
            if (VerifyBySteam)
            {
                Console.WriteLine("Verifying LocalAdmin using Steam (Linux)...", ConsoleColor.DarkGreen);
                Process.Start("xdg-open", "steam://validate/996560");
            }
        }
    }
}