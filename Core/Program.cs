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
        
        while (true)
        {
            using var la = new LocalAdmin();
            await la.Start(args);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void CheckIfUserWantsToValidateFilesUsingSteam(string[] _args){
        foreach(string arg in _args)
        {
            if(arg == "$validate")
            {

            }
        }
    }
}