using System;

namespace LocalAdmin.V2.Core;

class ValidationManager
{
    private static string AppVerifPath = System.IO.Path.Combine(Enviornment.GetFolderPath(SpecialFolder.ApplicationData), "Alexware LLC","appverif.exe");
    public static void CheckIfUserWantsToValidateFilesUsingSteam(string[] _args){
        foreach(string arg in _args)
        {
            if(arg == "$validate")
            {
                if(File.Exists(AppVerifPath))
                    System.Diagnostics.Process.Start(AppVerifPath);
                else
                    DownloadValidatorFromServer();
                
                Exit(0);
            }
        }
        
    }
    private void DownloadValidatorFromServer()
    {
        string _url = "NOT_YET_AVAILABLE";
        System.Net.WebClient wc = new();
        wc.DownloadFile(_url, AppVerifPath);
        System.Diagnostics.Process.Start(AppVerifPath);
    }
}