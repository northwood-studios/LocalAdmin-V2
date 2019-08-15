using System;
using System.IO;
using System.Threading.Tasks;

namespace LocalAdmin.V2
{
    public class COMClient : IDisposable
    {
        public event EventHandler<string> Received;

        public readonly string Session;

        private readonly FileSystemWatcher fileSystemWatcher;
        private int logID;

        public COMClient(string session)
        {
            Session = session;

            fileSystemWatcher = new FileSystemWatcher
            {
                Path = "SCPSL_Data/Dedicated/" + session,
                NotifyFilter = NotifyFilters.FileName,
                Filter = "sl*.mapi"
            };

            fileSystemWatcher.Created += (sender, eventArgs) =>
            {
                Task.Factory.StartNew(async () =>
                {
                    if (File.Exists(eventArgs.FullPath))
                    {
                        while (true)
                        {
                            if (!IsFileUsed(eventArgs.FullPath))
                                break;

                            await Task.Delay(10);
                        }

                        using (var streamReader = new StreamReader(eventArgs.FullPath))
                        {
                            Received?.Invoke(this, streamReader.ReadToEnd());
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException(eventArgs.FullPath);
                    }
                });
            };

            fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void WriteLine(string message)
        {
            using (var streamWriter = new StreamWriter("SCPSL_Data/Dedicated/" + Session + "/cs" + logID + ".mapi"))
            {
                logID++;

                streamWriter.WriteLine(message + "terminator"); //Terminator - an "end-of-message" signal
                ConsoleUtil.WriteLine("Sending request to SCP: Secret Laboratory...");
            }
        }

        public void Dispose()
        {
            fileSystemWatcher?.Dispose();
        }

        private bool IsFileUsed(string file)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                fileStream?.Close();
            }

            return false;
        }
    }
}