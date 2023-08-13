using LocalAdmin.V2.IO;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Core;
using System;

namespace LocalAdmin.V2.Commands;

internal sealed class LASettingsCommand : CommandBase
{
    Config Settings;
    public LASettingsCommand() : base("settings") { }
    internal override void Execute(string[] arguments)
    {
        Settings = LocalAdmin.V2.Core.LocalAdmin.Configuration;
        ConsoleUtil.WriteLine(" |=====| Settings |=====| ");
        Console.WriteLine();
        GetConfigSetting();
    }
    private void GetConfigSetting()
    {
        ConsoleUtil.WriteLine(Settings.ToString());
    }
}
