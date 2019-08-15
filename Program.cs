namespace LocalAdmin.V2
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var localAdmin = new LocalAdmin();

            localAdmin.Start(args);
        }
    }
}