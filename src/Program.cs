using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BulkEditGoogleDrive
{
    /// <typeparam name="Scopes">Array of string containing the scopes the program should use.</typeparam>
    /// <typeparam name="ApplicationName">The name of the program.</typeparam>
    /// <typeparam name="Service">The Google Service instance to connect to reference Google.</typeparam>
    /// <exception cref="FileNotFoundException">
    /// If the file containing the user's credentials couldn't be found.
    /// </exception>
    class Program
    {
        static string[] Scopes = {
            DriveService.Scope.Drive,
            DriveService.Scope.DriveAppdata,
            DriveService.Scope.DriveFile,
        };
        static string ApplicationName = "BulkEditGoogleDrive";
        static DriveService? Service;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╔═════════════════════╗");
            Console.WriteLine("║ BulkEditGoogleDrive ║");
            Console.WriteLine("╚═════════════════════╝");
            Console.ResetColor();

            Console.WriteLine(
                "\nFill the \"files.txt\" file with the names and the extensions of the"
                + "files you want to create. Use the first line to specify the name of the"
                + "folder and the other lines to specify the names and the extensions of"
                + "the files."
            );

            Console.Write("Press any key to continue.");
            Console.ReadLine();

            // Read data from the "files.txt" file

            // Connect to Google Drive
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
                }

                Service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The \"credentials.json\" file was not found.");
                Console.ResetColor();

                Environment.Exit(1);
            }

            // Create files in Google Drive
        }

        /// <summary>
        /// Create the folder and the template on the user's Google Drive account
        /// based on the names provided.
        /// </summary>
        public static void CreateFolderAndTemplate(string folderName, string templateName)
        {
            try
            {
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                var request = Service!.Files.Create(folderMetadata);
                var folderId = request.Execute();

                var templateMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = $"{templateName}",
                    Parents = new List<string>
                    {
                        folderId.Id
                    },
                    MimeType = "application/vnd.google-apps.spreadsheet"
                };

                Service.Files.Create(templateMetadata).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                Environment.Exit(1);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("The folder and the Google Sheet file were created successfully.");
            Console.ResetColor();
        }
    }
}
