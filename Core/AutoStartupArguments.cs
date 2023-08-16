using System.IO;

namespace LocalAdmin.V2.Core;

internal class AutoStartupArguments
{
    public static string[] ReadStartupArgs()
    {
        string readallargs = File.ReadAllText("startup.txt");
        return readallargs.Split(' ');
    }
}