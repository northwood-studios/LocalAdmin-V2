using System.Threading.Tasks;

namespace LocalAdmin.V2.Core;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        while (true)
        {
            using var la = new LocalAdmin();
            await la.Start(args);
        }
        // ReSharper disable once FunctionNeverReturns
    }
}
