using System;
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
        ValidationManager.CheckIfUserWantsToValidateFilesUsingSteam();
        while (true)
        {
            using var la = new LocalAdmin();
            await la.Start(args);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    
}