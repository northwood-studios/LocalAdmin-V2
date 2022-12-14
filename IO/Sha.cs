using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LocalAdmin.V2.IO;

internal static class Sha
{
    internal static string Sha256File(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sha256 = SHA256.Create();
        return HashToString(sha256.ComputeHash(fs));
    }

    private static string HashToString(byte[] hash)
    {
        var sb = new StringBuilder();
        foreach (var t in hash)
            sb.Append(t.ToString("X2"));
        return sb.ToString();
    }
}