using System;
using System.IO;
using System.IO.Compression;

namespace LocalAdmin.V2.IO.Logging
{
    public static class Logger
    {
        public static TextWriter? Writer { get; private set; }

        public static void Setup(ushort port)
        {
            var directoryPath = Path.Combine("logs", port.ToString());
            var filePath = Path.Combine(directoryPath, "latest.log");

            Directory.CreateDirectory(directoryPath);
            if (File.Exists(filePath))
            {
                var creationTime = File.GetCreationTime(filePath).ToString("yyyy-MM-dd");
                var count = Directory.GetFiles(directoryPath, $"{creationTime}-*.log.gz").Length + 1;

                using (var originalFileStream = File.OpenRead(filePath))
                {
                    using var compressedFileStream = File.Create(Path.Combine(directoryPath, $"{creationTime}-{count}.log.gz"));
                    using var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);

                    originalFileStream.CopyTo(compressionStream);
                }

                File.Delete(filePath);
            }

            Writer = File.AppendText(filePath);
        }

        public static void Log(object obj)
        {
            if (Writer == null)
                return;

            var content = obj.ToString();
            content = string.IsNullOrEmpty(content) ? string.Empty : $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {content}";

            Writer.Write(content);
            Writer.Flush();
        }

        public static void LogLine(object obj)
        {
            Log(obj + Environment.NewLine);
        }
    }
}