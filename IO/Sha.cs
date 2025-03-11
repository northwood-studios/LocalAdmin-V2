using System;
using System.IO;
using System.Security.Cryptography;

namespace LocalAdmin.V2.IO;

internal static class Sha
{
    internal static string Sha256File(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return Convert.ToHexString(SHA256.HashData(fs));
    }
}