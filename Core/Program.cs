namespace LocalAdmin.V2.Core
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (var la = LocalAdmin.Singleton)
            {
                la.Start(args);
            }
        }
    }
}