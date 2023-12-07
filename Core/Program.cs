using System.IO;
using System.Threading.Tasks;

namespace LocalAdmin.V2.Core;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Utf8Json.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
            Utf8Json.Resolvers.GeneratedResolver.Instance,
            Utf8Json.Resolvers.BuiltinResolver.Instance
        );

        while (true)
        {
            using var la = new LocalAdmin();

            if (File.Exists("laargs.txt"))
                StartupArgManager.MigrateArgsFile();

            await la.Start(StartupArgManager.MergeStartupArgs(args));
        }
        // ReSharper disable once FunctionNeverReturns
    }
}