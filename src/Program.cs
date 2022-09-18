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
using Microsoft.Win32;

namespace DriveQuickstart
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("══════════════════════════");
            Console.WriteLine("ManageFilesFromGoogleSheet");
            Console.WriteLine("══════════════════════════\n");

            Console.WriteLine("Choose the option you want:");
            Console.WriteLine("(1) Create a new folder/template registry.");
            Console.WriteLine("(2) Update a folder/template registry.");
            Console.WriteLine("(3) Delete the data of the application.\n");
            Console.Write("Your option: ");

            byte decision = 0;
            try
            {
                decision = Convert.ToByte(Console.ReadLine());
            }
            catch (FormatException e)
            {
                Console.WriteLine(e);
                return;
            }
            catch (OverflowException e)
            {
                Console.WriteLine(e);
                return;
            }

            switch (decision)
            {
                case 1:
                    Console.WriteLine("Create a new folder/template registry.");

                    Console.Write("Input the name of the folder: ");
                    string? folderName = Console.ReadLine();
                    Console.Write("Input the name of the template file: ");
                    string? templateName = Console.ReadLine();

                    RegistryStorage.SaveToRegistry(folderName!, templateName!);

                    DriveManagement.ConnectToGoogle();
                    DriveManagement.CreateFolder(folderName!);

                    break;
                case 2:
                    break;
                case 3:
                    Console.WriteLine("Delete the data of the application.");
                    Console.WriteLine("All the data from the app will be deleted from the Windows registry.");
                    Console.Write("Are you sure? (y/n): ");
                    char deleteOrNot = Convert.ToChar(Console.Read());

                    if (Char.ToUpper(deleteOrNot) == 'Y')
                        RegistryStorage.DeleteAppData();
                    else
                        break;

                    break;
                default:
                    Console.WriteLine("Only \"1\", \"2\", and \"3\" are valid options.");
                    break;
            }

            return;
        }
    }

    public class DriveManagement
    {
        static string[] Scopes = {
            DriveService.Scope.Drive,
            DriveService.Scope.DriveAppdata,
            DriveService.Scope.DriveFile,
        };
        static string ApplicationName = "ManageFilesFromGoogleSheet";
        static DriveService? Service;

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
                Console.WriteLine(e.Message);
            }
        }

        public static void CreateFolder(string folderName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = Service!.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();

            Console.WriteLine("Folder ID: " + file.Id);
        }
    }

    public class RegistryStorage
    {
        public static void SaveToRegistry(string folderName, string templateName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ManageFilesFromGoogleSheet");
            key.SetValue(folderName, templateName);
            key.Close();
        }
        
        public static string[] ReadFromRegistry()
        {
            return;
        }
        public static string ReadFromRegistry(string registryPair)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ManageFilesFromGoogleSheet");
            return key.GetValue(registryPair).ToString();
        }

        public static void DeleteAppData()
        {
            Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\ManageFilesFromGoogleSheet");
        }
    }
}