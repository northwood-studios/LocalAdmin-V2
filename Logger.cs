namespace LocalAdmin_V2_Net_Core
{
    public static class Logger
    {
        public static void Log(string text)
        {
            /*if (!File.Exists("localadmin.log")) File.Create("localadmin.log");

            using (var streamWriter = new StreamWriter("localadmin.log"))
            {
                streamWriter.WriteLine(File.ReadAllText("localadmin.log"));
                streamWriter.WriteLine(text);
            }*/
        }

        public static void Log(object obj)
        {
            /*if (!File.Exists("localadmin.log")) File.Create("localadmin.log");

            using (var streamWriter = new StreamWriter("localadmin.log"))
            {
                streamWriter.WriteLine(File.ReadAllText("localadmin.log"));
                streamWriter.WriteLine(obj.ToString());
            }*/
        }
    }
}