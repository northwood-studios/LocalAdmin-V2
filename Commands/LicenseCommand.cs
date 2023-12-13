using LocalAdmin.V2.Commands.Meta;
using LocalAdmin.V2.IO;
using System;

namespace LocalAdmin.V2.Commands;

internal sealed class LicenseCommand : CommandBase
{
    public LicenseCommand() : base("License", "Prints LocalAdmin license details.") { }

    internal override void Execute(string[] arguments)
    {
        ConsoleUtil.WriteLine("MIT License", ConsoleColor.Cyan);
        ConsoleUtil.WriteLine("Copyright by Łukasz \"zabszk\" Jurczyk and KernelError, 2019 - 2023", ConsoleColor.Gray);
        ConsoleUtil.WriteLine("Permission is hereby granted, free of charge, to any person obtaining a copy\r\nof this software and associated documentation files (the \"Software\"), to deal\r\nin the Software without restriction, including without limitation the rights\r\nto use, copy, modify, merge, publish, distribute, sublicense, and/or sell\r\ncopies of the Software, and to permit persons to whom the Software is\r\nfurnished to do so, subject to the following conditions:", ConsoleColor.Gray);
        ConsoleUtil.WriteLine("\r\nThe above copyright notice and this permission notice shall be included in all\r\ncopies or substantial portions of the Software.", ConsoleColor.Gray);
        ConsoleUtil.WriteLine("\r\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\r\nIMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,\r\nFITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE\r\nAUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER\r\nLIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,\r\nOUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE\r\nSOFTWARE.", ConsoleColor.Gray);

        ConsoleUtil.WriteLine("", ConsoleColor.Gray);
        ConsoleUtil.WriteLine("LocalAdmin includes Utf8Json developed by Yoshifumi Kawai licensed under The MIT License.", ConsoleColor.Gray);
    }
}