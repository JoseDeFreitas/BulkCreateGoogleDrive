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

namespace BulkEditGoogleDrive
{
    class Program
    {
        static string[] Scopes = { DriveService.Scope.DriveFile };
        static DriveService Service;
        static Dictionary<string, string> Extensions = new Dictionary<string, string>()
        {
            {"gdo", "application/vnd.google-apps.document"}, {"gsh", "application/vnd.google-apps.spreadsheet"},
            {"gsl", "application/vnd.google-apps.presentation"}, {"gfo", "application/vnd.google-apps.form"}
        };

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╔═════════════════════╗");
            Console.WriteLine("║ BulkEditGoogleDrive ║");
            Console.WriteLine("╚═════════════════════╝");
            Console.ResetColor();

            Console.WriteLine(
                "To know how to format the file, read the description in "
                + "https://github.com/JoseDeFreitas/BulkEditGoogleDrive."
            );

            Console.Write("Do you want to update the information of the file? (y/n): ");

            char decision = 'n';
            try
            {
                decision = Convert.ToChar(Console.ReadLine());
            }
            catch (Exception e)            
            {                
                if (e is ArgumentNullException || e is FormatException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The answer should be \"y\" or \"n\".");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
            }

            if (decision == 'y')
            {
                try
                {
                    Process.Start("notepad.exe", "files.txt");
                }
                catch (Win32Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The notepad application could not be found.");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
                
                Environment.Exit(0);
            }

            // Read data from the "files.txt" file
            string[] fileNames = {};
            try
            {
                fileNames = System.IO.File.ReadAllLines("files.txt");
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file \"files.txt\" was not found.");
                Console.ResetColor();
                Environment.Exit(1);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An IO error ocurred when opening the file.");
                Console.ResetColor();
                Environment.Exit(1);
            }

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
                    ApplicationName = "BulkEditGoogleDrive"
                });
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The \"credentials.json\" file was not found.");
                Console.ResetColor();
                Environment.Exit(1);
            }

            // Define names and options
            string folderName = "";
            char numberedNames = 'n';
            try
            {
                folderName = fileNames[0].Split('|')[0];
                numberedNames = Convert.ToChar(fileNames[0].Split('|')[1]);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException || e is ArgumentOutOfRangeException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The answer should be \"y\" or \"n\".");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
            }

            // Create files in Google Drive
            int createdFilesCount = 0;
            int skippedFilesCount = 0;
            try
            {
                // Check if folder already exists
                string folderId = "";

                var folderRequest = Service.Files.List();
                folderRequest.Q = $"name='{folderName}' and mimeType='application/vnd.google-apps.folder'";
                var folderResult = folderRequest.Execute();

                if (folderResult.Files.Count == 0)
                {
                    var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = folderName,
                        MimeType = "application/vnd.google-apps.folder"
                    };

                    var request = Service.Files.Create(folderMetadata).Execute();
                    folderId = request.Id;
                }
                else
                    folderId = folderResult.Files[0].Id;

                // Create files inside folder
                for (int i = 1; i < fileNames.Length; i++)
                {
                    string fileName = "";
                    try
                    {
                        fileName = fileNames[i].Split('.')[0];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("The format of the \"files.txt\" file is wrong.");
                        Console.ResetColor();
                        Environment.Exit(1);
                    }

                    if (numberedNames == 'y')
                        fileName = $"{i}- {fileName}";

                    string fileType = "";
                    try
                    {
                        fileType = Extensions[fileNames[i].Split('.')[1]];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("The format of the \"files.txt\" file is wrong.");
                        Console.ResetColor();
                        Environment.Exit(1);
                    }

                    // Check if file already exists
                    var fileRequest = Service.Files.List();
                    fileRequest.Q = $"name='{fileName}' and mimeType='{fileType}'";
                    var fileResult = fileRequest.Execute();

                    if (fileResult.Files.Count == 0)
                    {
                        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                        {
                            Name = fileName,
                            Parents = new List<string>
                            {
                                folderId
                            },
                            MimeType = fileType
                        };

                        Service.Files.Create(fileMetadata).Execute();
                        createdFilesCount++;
                    }
                    else
                        skippedFilesCount++;
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The folder or the files couldn't be created.");
                Console.ResetColor();
                Environment.Exit(1);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"{createdFilesCount} files were created successfully.\n"
                + $"{skippedFilesCount} files were skipped because they already existed."
            );
            Console.ResetColor();
        }
    }
}
