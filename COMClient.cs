using System;
using System.IO;
using System.Threading.Tasks;

namespace LocalAdmin.V2
{
    public class COMClient : IDisposable
    {
        private readonly FileSystemWatcher fileSystemWatcher;
        public readonly string Session;

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
                Task.Factory.StartNew(() =>
                {
                    if (File.Exists(eventArgs.FullPath))
                    {
                        while (IsFileUsed(eventArgs.FullPath))
                        {
                        }

                        using (var streamReader = new StreamReader(eventArgs.FullPath))
                        {
                            if (Received != null) Received(this, streamReader.ReadToEnd());
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

        public void Dispose()
        {
            fileSystemWatcher?.Dispose();
        }

        public event EventHandler<string> Received;

        public void Write(string message)
        {
            using (var sw = new StreamWriter("SCPSL_Data/Dedicated/" + Session + "/cs" + logID + ".mapi"))
            {
                logID++;
                sw.WriteLine(message + "terminator"); //Terminator - an "end-of-message" signal
                ConsoleUtil.WriteLine("Sending request to SCP: Secret Laboratory...");
            }
        }

        public void ResetLogID()
        {
            logID = 0;
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