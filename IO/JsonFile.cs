using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;

namespace LocalAdmin.V2.IO;

internal static class JsonFile
{
    internal static async Task<T?> Load<T>(string path, uint timeout = 2000, bool keepLock = false)
    {
        try
        {
            bool lockGranted = await LockFile(path, timeout);
            var result = await Task.FromResult(JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path, Encoding.UTF8)));
            
            if (lockGranted && !keepLock)
                UnlockFile(path);
            
            return result;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to load file {path}. Exception: {e.Message}", ConsoleColor.Red);
            ConsoleUtil.WriteLine($"Stack trace: {e.StackTrace}");

            if (e.InnerException != null)
            {
                ConsoleUtil.WriteLine($"Inner exception: {e.InnerException.Message}");
                ConsoleUtil.WriteLine($"Inner exception: {e.InnerException.StackTrace}");
            }

            return default;
        }
    }

    internal static async Task<bool> TrySave<T>(this T obj, string path, uint timeout = 2000, bool forceUnlock = false) where T : class
    {
        try
        {
            bool lockGranted = await LockFile(path, timeout);
            await File.WriteAllTextAsync(path, JsonSerializer.ToJsonString(obj), Encoding.UTF8);
            
            if (lockGranted || forceUnlock)
                UnlockFile(path);

            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to save file {path}. Exception: {e.Message}", ConsoleColor.Red);
            return false;
        }
    }

    private static async Task<bool> LockFile(string path, uint timeout)
    {
        try
        {
            path += ".lock";
            timeout /= 10;
        
            if (timeout < 1)
                timeout = 1;
        
            for (var i = 0; i < timeout && File.Exists(path); i++)
                await Task.Delay(10);

            if (File.Exists(path))
                return false;
            
            var fs = File.Create(path);
            fs.Close();
            return true;
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to process lock {path}. Exception: {e.Message}", ConsoleColor.Red);
            return false;
        }
    }
    
    internal static void UnlockFile(string path)
    {
        try
        {
            path += ".lock";
            FileUtils.DeleteIfExists(path);
        }
        catch (Exception e)
        {
            ConsoleUtil.WriteLine($"Failed to process unlock {path}. Exception: {e.Message}", ConsoleColor.Red);
        }
    }
}
