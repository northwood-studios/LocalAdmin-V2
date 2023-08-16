using System.Threading.Tasks;

namespace LocalAdmin.V2.Core;

internal class AutoStartupArguments
{
    public static string[] ReadStartupArgs()
    {
        string readallargs = File.ReadAllText("startup.txt");
        return readallargs.Split(' ');
    }
}