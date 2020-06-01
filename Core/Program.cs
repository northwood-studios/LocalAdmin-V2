namespace LocalAdmin.V2.Core
{
    internal class Program
    {
        internal static LocalAdmin? localAdmin;

        public static void Main(string[] args)
        {
            localAdmin = new LocalAdmin();

            localAdmin.Start(args);
        }
    }
}