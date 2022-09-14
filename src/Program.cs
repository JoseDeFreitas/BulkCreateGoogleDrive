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
        static string[] Scopes = { DriveService.Scope.Drive };
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

                CreateFolder(service);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void CreateFolder(DriveService service)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "Test",
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();

            Console.WriteLine("Folder ID: " + file.Id);
        }
    }
}