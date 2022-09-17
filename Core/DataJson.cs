using System;
using System.IO;
using System.Text;
using LocalAdmin.V2.IO;
using Newtonsoft.Json;

namespace LocalAdmin.V2.Core;

internal class DataJson
{
    [JsonProperty("EulaAccepted")]
    public DateTime? EulaAccepted;

    internal static bool TryLoad(string path, out DataJson? value)
    {
        try
        {
            value = JsonConvert.DeserializeObject<DataJson>(File.ReadAllText(path, Encoding.UTF8));
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to load file {path}. Exception: {e.Message}", ConsoleColor.Red);
            value = null;
            return false;
        }
    }

    internal bool TrySave(string path)
    {
        try
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented), Encoding.UTF8);
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to save file {path}. Exception: {e.Message}", ConsoleColor.Red);
            return false;
        }
    }
}