using System.IO;

namespace LocalAdmin.V2.IO;

internal static class FileUtils
{
    internal static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
    
    internal static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }
}