using System.IO;

namespace LocalAdmin.V2.IO;

internal static class FileUtils
{
    internal static bool DeleteIfExists(string path)
    {
        if (!File.Exists(path))
            return false;

        File.Delete(path);
        return true;
    }

    internal static bool DeleteDirectoryIfExists(string path)
    {
        if (!Directory.Exists(path))
            return false;

        Directory.Delete(path, true);
        return true;
    }
}