using System;
using System.IO;
using System.Text;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.Core
{
    public static class ConfigWizard
    {
        public static void RunConfigWizard()
        {
            var curConfig = LocalAdmin.Configuration;
            
            Retry:
            Console.WriteLine("Welcome to LocalAdmin Configuration Wizard!");
            Console.WriteLine();
            Console.WriteLine(
                "We will ask you a couple of questions. You can always change your answers by running LocalAdmin with \"--reconfigure\" argument or manually editing configuration files in " +
                LocalAdmin.GameUserDataRoot + "config directory.");
            Console.WriteLine();
            
            Console.WriteLine(LocalAdmin.Configuration == null ? "This is the default LocalAdmin configuration:" : "That's your current LocalAdmin configuration:");
            LocalAdmin.Configuration ??= new Config();
            Console.WriteLine(LocalAdmin.Configuration.ToString());
            
            Console.WriteLine();
            var input = "0";
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
            LocalAdmin.Configuration.LaShowStdoutStderr = BoolInput("Should standard outputs (contain a lot of debug information) be visible on the LocalAdmin console?");
            LocalAdmin.Configuration.LaNoSetCursor = BoolInput("Should cursor position management be DISABLED (disable only if you are experiencing issues with the console, may cause issues especially on linux)?");
            LocalAdmin.Configuration.EnableLaLogs = BoolInput("Do you want to enable LocalAdmin logs?");

            if (LocalAdmin.Configuration.EnableLaLogs)
            {
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
                var input = Console.ReadLine();
                
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
                var input = Console.ReadLine();
                
                if (input == null) continue;
                if (ushort.TryParse(input, out var u))
                    return u;
            }
        }

        private static void SaveConfig()
        {
            var input = "0";
            while (!string.IsNullOrWhiteSpace(input) && !input.Equals("this", StringComparison.OrdinalIgnoreCase) &&
                   !input.Equals("global", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Do you want to save the configuration only for THIS server (on port {LocalAdmin.GamePort} or should it become a GLOBAL configuration (default one for all future servers - servers not configured yet)? [this/global]: ");
                input = Console.ReadLine();
            }

            var cfgPath =
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