using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace DriveQuickstart
{
    class Program
    {
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "ManageFilesFromGoogleSheet";

        static void Main(string[] args)
        {
            Console.WriteLine("══════════════════════════");
            Console.WriteLine("ManageFilesFromGoogleSheet");
            Console.WriteLine("══════════════════════════");
            
            var decision = "";

            while ((decision != "y") || (decision != "n"))
            {
                Console.Write("Are these settings correct? (y/n): ");
                decision = Console.ReadLine();

                if (decision == "y")
                {
                    ConnectToGoogle();
                    return;
                }
                else if (decision == "n")
                {
                    try
                    {
                        Process.Start("notepad.exe", "options.txt");
                    }
                    catch (Win32Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine(e);
                    }

                    return;
                }
            }
        }

        static void ConnectToGoogle()
        {
            try
            {
                UserCredential credential;

                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine($"Credential file saved to: {credPath}");
                }

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";

                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                    .Files;
                Console.WriteLine("Files:");

                if (files == null || files.Count == 0)
                {
                    Console.WriteLine("No files found.");
                    return;
                }

                foreach (var file in files)
                {
                    Console.WriteLine($"{file.Name} ({file.Id})");
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}