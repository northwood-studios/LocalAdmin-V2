using LocalAdmin.V2.IO;
using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.Core;
using System;

namespace LocalAdmin.V2.Commands;

internal sealed class LASettingsCommand : CommandBase
{
    Config Settings;
    public LASettingsCommand() : base("settings") { }
    internal override void Execute(string[] arguments) => ConsoleUtil.WriteLine(Settings.ToString());
}
