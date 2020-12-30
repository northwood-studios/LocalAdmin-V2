namespace LocalAdmin.V2.Core
{
    static class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                using var la = new LocalAdmin();
                la.Start(args);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}