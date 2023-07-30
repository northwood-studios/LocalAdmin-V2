// WebTools | v0.0.1 | C# 7.0
//
//By ALEXWARELLC | https://github.com/ALEXWARELLC
//This file has been modified by ALEXWARELLC for use in 'LocalAdmin V2 (Northwood Studios)'
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LocalAdmin.V2.IO;

namespace LocalAdmin.V2.NET
{
    public class WebTools
    {

        //NOTE: TestMethod namespace isn't included. No tests can be done.

        /// <summary>
        /// Downloads a file asynchronously.
        /// </summary>
        /// <param name="URL">The path of a file from a webserver to download.</param>
        /// <param name="DestinationPath">The destination of the URL content download.</param>
        /// <returns></returns>
        public static async Task DownloadFileAsync(string URL, string DestinationPath)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(URL))
                    {
                        response.EnsureSuccessStatusCode();
                        using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fileStream = File.Create(DestinationPath))
                            {
                                byte[] buffer = new byte[8192];
                                int bytesRead;
                                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                }
                ConsoleUtil.WriteLine($"Download Successful - '{Path.GetFileName(DestinationPath)}' from {URL}", ConsoleColor.DarkGreen);
                return;
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"An error occured while trying to complete download. '{ex.ToString()}'", ConsoleColor.Red);
                return;
            }
        }
        /// <summary>
        /// Compares a file to a specified hash to check it has not been tampered with during a download operation.
        /// </summary>
        /// <param name="FilePath">The path to a valid and accessable file to validate.</param>
        /// <param name="CompareHash">The hash to compare a file to.</param>
        /// <returns>File Intergity (True/False)</returns>
        public static bool CheckFileIntegrity(string FilePath, string CompareHash)
        {
            ConsoleUtil.Write($"Validating file: {FilePath}... ");
            try
            {
                if (!File.Exists(FilePath))
                {
                    new FileNotFoundException("The specified file path does not exist. Check the file path and try again.");
                    return false;
                }
                byte[] fileData = File.ReadAllBytes(FilePath);
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] actualHash = sha256.ComputeHash(fileData);
                    string actualHashString = BitConverter.ToString(actualHash).Replace("-", "").ToLower();
                    if (actualHashString == CompareHash.ToLower())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("OK");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FAILED");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAILED - VALIDATOR ISSUE");
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }
        }
    }
}
