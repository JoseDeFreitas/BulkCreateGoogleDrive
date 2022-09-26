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
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╔═════════════════════╗");
            Console.WriteLine("║ BulkEditGoogleDrive ║");
            Console.WriteLine("╚═════════════════════╝");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Class that contains the methods that make possible the connection with Google Drive,
    /// and that create and update the files.
    /// </summary>
    /// <typeparam name="Scopes">Array of string containing the scopes the program should use.</typeparam>
    /// <typeparam name="ApplicationName">The name of the program.</typeparam>
    /// <typeparam name="Service">The Google Service instance to connect to reference Google.</typeparam>
    public class DriveManagement
    {
        static string[] Scopes = {
            DriveService.Scope.Drive,
            DriveService.Scope.DriveAppdata,
            DriveService.Scope.DriveFile,
        };
        static string ApplicationName = "BulkEditGoogleDrive";
        static DriveService? Service;

        /// <summary>
        /// Reads the user's credentials from the <c>credentials.json</c> file they
        /// should have included in the root folder, where the executable file of the
        /// program lies.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// If the file containing the user's credentials couldn't be found.
        /// </exception>
        public static void ConnectToGoogle()
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
            catch (FileNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The \"credentials\" file was not found.\n\n{e}");
                Console.ResetColor();

                Environment.Exit(1);
            }
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
