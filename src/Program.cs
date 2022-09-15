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
        static string[] Scopes = {
            DriveService.Scope.Drive,
            DriveService.Scope.DriveAppdata,
            DriveService.Scope.DriveFile,
        };
        static string ApplicationName = "ManageFilesFromGoogleSheet";
        static string FolderName = "";
        static string TemplateSheet = "";

        static void Main(string[] args)
        {
            Console.WriteLine("══════════════════════════");
            Console.WriteLine("ManageFilesFromGoogleSheet");
            Console.WriteLine("══════════════════════════\n");

            Console.Write("Input the name of the folder you want (won't be created if already exists): ");
            string? FolderName = Console.ReadLine();
            Console.WriteLine("\n");
            Console.Write("Input the name of the template sheet (won't be created if already exists): ");
            string? TemplateSheet = Console.ReadLine();

            try
            {
                UserCredential credential;

                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "admin",
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

            return;
        }

        static void CreateFolder(DriveService service)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = FolderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();

            Console.WriteLine("Folder ID: " + file.Id);
        }
    }
}