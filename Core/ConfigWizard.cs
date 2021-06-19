using LocalAdmin.V2.IO;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace LocalAdmin.V2.Core
{
    public static class ConfigWizard
    {
        public static void RunConfigWizard(bool useDefault)
        {
            if (useDefault)
            {
                LocalAdmin.Configuration ??= new Config();
                SaveConfig(true);
                return;
            }

            Config? curConfig = LocalAdmin.Configuration;

        Retry:
            Console.WriteLine("Welcome to LocalAdmin Configuration Wizard!");
            Console.WriteLine();
            Console.WriteLine(
                "We will ask you a couple of questions. You can always change your answers by running LocalAdmin with \"--reconfigure\" argument or manually editing configuration files in " +
                (LocalAdmin.ConfigPath ?? LocalAdmin.GameUserDataRoot) + "config directory.");
            Console.WriteLine();

            Console.WriteLine(LocalAdmin.Configuration == null ? "This is the default LocalAdmin configuration:" : "That's your current LocalAdmin configuration:");
            LocalAdmin.Configuration ??= new Config();
            Console.WriteLine(LocalAdmin.Configuration.ToString());

            Console.WriteLine();
            string? input = "0";
            while (!string.IsNullOrWhiteSpace(input) && !input.Equals("edit", StringComparison.OrdinalIgnoreCase) &&
                   !input.Equals("keep", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Do you want to edit that configuration? [edit/keep]: ");
                input = Console.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(input) || input.Equals("keep", StringComparison.OrdinalIgnoreCase))
            {
                SaveConfig();
                return;
            }

            LocalAdmin.Configuration.RestartOnCrash = BoolInput("Should the server be automatically restarted after a crash?");
            LocalAdmin.Configuration.LaLiveViewUseUtc = !BoolInput("Should timestamps in the LocalAdmin live view use server timezone (otherwise UTC time will be use)?");

            if (BoolInput("Do you want to customize time format of timestamps in the LocalAdmin live view?"))
            {
                DateTime dt = DateTime.Now;

                Console.WriteLine($"1. Full timestamp: {dt.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)} (yyyy-MM-dd HH:mm:ss.fff zzz)");
                Console.WriteLine($"2. Full timestamp w/o timezone: {dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} (yyyy-MM-dd HH:mm:ss.fff)");
                Console.WriteLine($"3. Full timestamp w/o milliseconds: {dt.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture)} (yyyy-MM-dd HH:mm:ss zzz)");
                Console.WriteLine($"4. Full timestamp w/o date: {dt.ToString("HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)} (HH:mm:ss.fff zzz)");
                Console.WriteLine($"5. Full timestamp w/o date and timezone: {dt.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)} (HH:mm:ss.fff)");
                Console.WriteLine($"6. Full timestamp w/o date and milliseconds: {dt.ToString("HH:mm:ss zzz", CultureInfo.InvariantCulture)} (HH:mm:ss zzz)");
                Console.WriteLine($"7. Date and short time: {dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} (yyyy-MM-dd HH:mm:ss)");
                Console.WriteLine($"8. Date, short time and timezone: {dt.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture)} (yyyy-MM-dd HH:mm:ss zzz)");
                Console.WriteLine($"9. Date, short time and milliseconds: {dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} (yyyy-MM-dd HH:mm:ss.fff)");
                Console.WriteLine("10. Custom");

                Console.WriteLine("Please choose a timestamp format by specifying its number.");

                bool tzPassed = false;

                while (!tzPassed)
                {
                    string? inputx = Console.ReadLine();

                    if (inputx == null || !ushort.TryParse(inputx, out ushort c)) continue;

                    switch (c)
                    {
                        case 1:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "yyyy-MM-dd HH:mm:ss.fff zzz";
                            tzPassed = true;
                            break;

                        case 2:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
                            tzPassed = true;
                            break;

                        case 3:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";
                            tzPassed = true;
                            break;

                        case 4:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "HH:mm:ss.fff zzz";
                            tzPassed = true;
                            break;

                        case 5:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "HH:mm:ss.fff";
                            tzPassed = true;
                            break;

                        case 6:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "HH:mm:ss zzz";
                            tzPassed = true;
                            break;

                        case 7:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "yyyy-MM-dd HH:mm:ss";
                            tzPassed = true;
                            break;

                        case 8:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";
                            tzPassed = true;
                            break;

                        case 9:
                            LocalAdmin.Configuration.LaLiveViewTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
                            tzPassed = true;
                            break;

                        case 10:
                            while (true)
                            {
                                Console.WriteLine("Enter custom timestamp format.");
                                LocalAdmin.Configuration.LaLiveViewTimeFormat = Console.ReadLine()!;
                                if (!string.IsNullOrWhiteSpace(LocalAdmin.Configuration.LaLiveViewTimeFormat) &&
                                    BoolInput("Save the specified time format?"))
                                    break;
                            }

                            tzPassed = true;
                            break;
                    }
                }
            }

            LocalAdmin.Configuration.LaShowStdoutStderr = BoolInput("Should standard outputs (contain a lot of debug information) be visible on the LocalAdmin live view?");
            LocalAdmin.Configuration.LaNoSetCursor = BoolInput("Should cursor position management be DISABLED (disable only if you are experiencing issues with the console, may cause issues especially on linux)?");
            LocalAdmin.Configuration.EnableLaLogs = BoolInput("Do you want to enable LocalAdmin logs?");

            if (LocalAdmin.Configuration.EnableLaLogs)
            {
                LocalAdmin.Configuration.LaLogsUseUtc = !BoolInput("Should timestamps in the LocalAdmin logs use server timezone (otherwise UTC time will be use)?");
                LocalAdmin.Configuration.LaLogAutoFlush = BoolInput("Should LocalAdmin logs be automatically flushed (file updated in real time - may affect performance)?");
                LocalAdmin.Configuration.LaLogStdoutStderr = BoolInput("Do you want to enable standard outputs logging?");
                LocalAdmin.Configuration.LaDeleteOldLogs = BoolInput("Do you want to automatically delete old LocalAdmin logs (older than a specified amount of days)?");

                if (LocalAdmin.Configuration.LaDeleteOldLogs)
                    LocalAdmin.Configuration.LaLogsExpirationDays = UshortInput("How many days LocalAdmin logs should be kept?");
            }

            LocalAdmin.Configuration.DeleteOldRoundLogs = BoolInput("Do you want to automatically delete old round logs (older than a specified amount of days)?");
            if (LocalAdmin.Configuration.DeleteOldRoundLogs)
                LocalAdmin.Configuration.RoundLogsExpirationDays = UshortInput("How many days round logs should be kept?");

            LocalAdmin.Configuration.CompressOldRoundLogs = BoolInput("Do you want to automatically compress old round logs (older than a specified amount of days)?");
            if (LocalAdmin.Configuration.CompressOldRoundLogs)
                LocalAdmin.Configuration.RoundLogsCompressionThresholdDays = UshortInput("How many days round logs should be kept uncompressed?");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Your new configuration:");
            Console.WriteLine(LocalAdmin.Configuration.ToString());
            Console.WriteLine();

            if (BoolInput("Save configuration?"))
            {
                SaveConfig();
                return;
            }

            Console.WriteLine();
            LocalAdmin.Configuration = curConfig;
            goto Retry;
        }

        private static bool BoolInput(string question)
        {
            while (true)
            {
                Console.WriteLine(question + " [yes/no]: ");
                string? input = Console.ReadLine();

                if (input == null) continue;
                if (input.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (input.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("no", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
        }

        private static ushort UshortInput(string question)
        {
            while (true)
            {
                Console.WriteLine(question + " ");
                string? input = Console.ReadLine();

                if (input == null) continue;
                if (ushort.TryParse(input, out ushort u))
                    return u;
            }
        }

        private static void SaveConfig(bool silent = false)
        {
            string? input = "0";

            if (LocalAdmin.ConfigPath != null)
                silent = true;

            if (!silent)
            {
                while (!string.IsNullOrWhiteSpace(input) && !input.Equals("this", StringComparison.OrdinalIgnoreCase) &&
                       !input.Equals("global", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        $"Do you want to save the configuration only for THIS server (on port {LocalAdmin.GamePort} or should it become a GLOBAL configuration (default one for all future servers - servers not configured yet)? [this/global]: ");
                    input = Console.ReadLine();
                }
            }
            else input = "this";

            if (LocalAdmin.ConfigPath != null)
            {
                DirectoryInfo? parent = Directory.GetParent(LocalAdmin.ConfigPath);

                if (parent == null)
                {
                    Console.WriteLine("FATAL ERROR: Can't create config directory (Directory processing error).");
                    Console.WriteLine("Path: " + LocalAdmin.ConfigPath);
                    Environment.Exit(1);
                    return;
                }

                if (!parent.Exists)
                {
                    try
                    {
                        Directory.CreateDirectory(parent.FullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FATAL ERROR: Can't create config directory.");
                        Console.WriteLine("Path: " + parent.FullName);
                        Console.WriteLine("Exception: " + e.Message);
                        Environment.Exit(1);
                        return;
                    }
                }

                if (File.Exists(parent.FullName))
                {
                    try
                    {
                        File.Delete(parent.FullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FATAL ERROR: Can't delete config file.");
                        Console.WriteLine("Path: " + parent.FullName);
                        Console.WriteLine("Exception: " + e.Message);
                        Environment.Exit(1);
                        return;
                    }
                }

                try
                {
                    File.WriteAllText(LocalAdmin.ConfigPath, LocalAdmin.Configuration!.SerializeConfig(), Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR: Can't write config file.");
                    Console.WriteLine("Path: " + LocalAdmin.ConfigPath);
                    Console.WriteLine("Exception: " + e.Message);
                    Environment.Exit(1);
                    return;
                }

                Console.WriteLine("Configuration saved!");
                return;
            }

            string? cfgPath =
                $"{LocalAdmin.GameUserDataRoot}config{Path.DirectorySeparatorChar}{LocalAdmin.GamePort}{Path.DirectorySeparatorChar}";

            if (!Directory.Exists(cfgPath))
            {
                try
                {
                    Directory.CreateDirectory(cfgPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR: Can't create config directory.");
                    Console.WriteLine("Path: " + cfgPath);
                    Console.WriteLine("Exception: " + e.Message);
                    Environment.Exit(1);
                    return;
                }
            }

            cfgPath += "config_localadmin.txt";

            if (input != null && input.Equals("this", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(cfgPath))
                {
                    try
                    {
                        File.Delete(cfgPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FATAL ERROR: Can't delete config file.");
                        Console.WriteLine("Path: " + cfgPath);
                        Console.WriteLine("Exception: " + e.Message);
                        Environment.Exit(1);
                        return;
                    }
                }

                try
                {
                    File.WriteAllText(cfgPath, LocalAdmin.Configuration!.SerializeConfig(), Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR: Can't write config file.");
                    Console.WriteLine("Path: " + cfgPath);
                    Console.WriteLine("Exception: " + e.Message);
                    Environment.Exit(1);
                    return;
                }

                Console.WriteLine("Configuration saved!");
                return;
            }

            if (File.Exists(cfgPath))
            {
                try
                {
                    File.Delete(cfgPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR: Can't delete **LOCAL** config file.");
                    Console.WriteLine("Path: " + cfgPath);
                    Console.WriteLine("Exception: " + e.Message);
                    Environment.Exit(1);
                    return;
                }
            }

            cfgPath = $"{LocalAdmin.GameUserDataRoot}config{Path.DirectorySeparatorChar}";

            if (!Directory.Exists(cfgPath))
            {
                try
                {
                    Directory.CreateDirectory(cfgPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR: Can't  **GLOBAL** config directory.");
                    Console.WriteLine("Path: " + cfgPath);
                    Console.WriteLine("Exception: " + e.Message);
                    Environment.Exit(1);
                    return;
                }
            }

            cfgPath += "config_localadmin_global.txt";

            if (File.Exists(cfgPath))
            {
                try
                {
                    File.Delete(cfgPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR: Can't delete **GLOBAL** config file.");
                    Console.WriteLine("Path: " + cfgPath);
                    Console.WriteLine("Exception: " + e.Message);
                    Environment.Exit(1);
                    return;
                }
            }

            try
            {
                File.WriteAllText(cfgPath, LocalAdmin.Configuration!.SerializeConfig(), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine("FATAL ERROR: Can't write **GLOBAL** config file.");
                Console.WriteLine("Path: " + cfgPath);
                Console.WriteLine("Exception: " + e.Message);
                Environment.Exit(1);
                return;
            }

            Console.WriteLine("Configuration saved!");
        }
    }
}