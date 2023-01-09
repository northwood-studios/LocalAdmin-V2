using System;
using LocalAdmin.V2.IO;
using LocalAdmin.V2.PluginsManager;

namespace LocalAdmin.V2.Commands.PluginManager.Subcommands;

internal static class TokenCommand
{
    internal static async void Token(string? token)
    {
        if (token == null)
        {
            var set = !string.IsNullOrEmpty(Core.LocalAdmin.DataJson!.GitHubPersonalAccessToken);

            ConsoleUtil.WriteLine(
                "[PLUGIN MANAGER] Setting GitHub Personal Access Token (PAT) is important, because it greatly increases rate limits.",
                ConsoleColor.DarkGray);
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Token is currently: " + (set ? "SET" : "NOT SET"),
                set ? ConsoleColor.DarkGreen : ConsoleColor.Yellow);
            ConsoleUtil.WriteLine("[PLUGIN MANAGER]", ConsoleColor.DarkGray);

            if (set)
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] To REMOVE PAT from LocalAdmin:", ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] 1. Run \"p token UNSET\".", ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine(
                    "[PLUGIN MANAGER] 2. Visit https://github.com/settings/tokens and delete the token you generated for LocalAdmin.",
                    ConsoleColor.DarkGray);
            }
            else
            {
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] To obtain PAT:", ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine(
                    "[PLUGIN MANAGER] 1. Visit https://github.com/settings/tokens (you may need to create a GitHub account).",
                    ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] 2. Click \"Generate new token\".", ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine(
                    "[PLUGIN MANAGER] 3. Optionally type a note, select expiration (we recommend \"No expiration\").",
                    ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] 4. DON'T select any scopes.", ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine("[PLUGIN MANAGER] 5. Click \"Generate token\".", ConsoleColor.DarkGray);
                ConsoleUtil.WriteLine(
                    "[PLUGIN MANAGER] 6. Copy the generated PAT and run \"p token GENERATED-PAT-HERE\" command.",
                    ConsoleColor.DarkGray);
            }

            return;
        }

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Reading LocalAdmin config file...", ConsoleColor.Blue);
        await Core.LocalAdmin.Singleton!.LoadJsonOrTerminate();

        Core.LocalAdmin.DataJson!.GitHubPersonalAccessToken = token.Equals("UNSET", StringComparison.OrdinalIgnoreCase) ? null : token;

        ConsoleUtil.WriteLine("[PLUGIN MANAGER] Writing LocalAdmin config file...", ConsoleColor.Blue);
        if (await Core.LocalAdmin.DataJson.TrySave(PathManager.InternalJsonDataPath))
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] GitHub Personal Access Token has been updated.", ConsoleColor.DarkGreen);
        else
            ConsoleUtil.WriteLine("[PLUGIN MANAGER] Failed to save data.json.", ConsoleColor.Red);

        PluginInstaller.RefreshPat();
    }
}