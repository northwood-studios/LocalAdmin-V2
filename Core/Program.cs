namespace LocalAdmin.V2.Core
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                using LocalAdmin? la = new LocalAdmin();
                la.Start(args);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}