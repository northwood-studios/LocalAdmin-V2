using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: DisableRuntimeMarshalling]

namespace LocalAdmin.V2.Core;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        while (true)
        {
            using var la = new LocalAdmin();
            await la.Start(StartupArgManager.MergeStartupArgs(args));
        }
        // ReSharper disable once FunctionNeverReturns
    }
}