using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace LocalAdmin.V2.IO;

internal static class FileUtils
{
    public static async ValueTask<FileStream> OpenAsync(string path, FileMode mode, FileAccess access, FileShare share, uint timeout = 2000)
    {
        const int retryTime = 50;
        long count = Math.DivRem(timeout, retryTime, out long remainder) - (remainder == 0 ? 1 : 0);
        for (long i = 0; i < count; i++)
        {
            try
            {
                return OpenCore(path, mode, access, share);
            }
            catch
            {
                await Task.Delay(retryTime);
            }
        }
        return OpenCore(path, mode, access, share);

        static FileStream OpenCore(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share, 4096, FileOptions.Asynchronous);
        }
    }

    public static async ValueTask<FileStream?> TryOpenAsync(string path, FileAccess access, FileShare share, uint timeout = 2000)
    {
        try
        {
            return await OpenAsync(path, FileMode.Open, access, share, timeout);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }
    }

    public static async ValueTask<string?> TryReadTextAsync(string path, FileShare share, uint timeout = 2000)
    {
        await using FileStream? stream = await TryOpenAsync(path, FileAccess.Read, share, timeout);
        if (stream == null)
            return null;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }

    public static async ValueTask<List<string>?> TryReadLinesAsync(string path, FileShare share, uint timeout = 2000)
    {
        string? text = await TryReadTextAsync(path, share, timeout);
        if (text == null)
            return null;

        List<string> lines = [];
        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
            lines.Add(line.ToString());
        return lines;
    }

    public static async ValueTask<T?> TryReadJsonAsync<T>(string path, FileShare share, JsonTypeInfo<T> json, uint timeout = 2000) where T : class
    {
        await using FileStream? stream = await TryOpenAsync(path, FileAccess.Read, share, timeout);
        if (stream == null)
            return null;
        return await JsonSerializer.DeserializeAsync(stream, json);
    }

    public static bool DeleteIfExists(string path)
    {
        bool existed = File.Exists(path);
        File.Delete(path);
        return existed;
    }

    public static bool DeleteDirectoryIfExists(string path)
    {
        try
        {
            Directory.Delete(path, true);
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }
}