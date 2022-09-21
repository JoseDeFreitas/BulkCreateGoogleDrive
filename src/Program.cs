﻿using Google.Apis.Auth.OAuth2;
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

namespace ManageFilesFromGoogleSheet
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔════════════════════════════╗");
            Console.WriteLine("║ ManageFilesFromGoogleSheet ║");
            Console.WriteLine("╚════════════════════════════╝\n");
            Console.ResetColor();

            PrintMenu();
        }

        static void PrintMenu()
        {
            Console.WriteLine("Choose the option you want:");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("(1) ");
            Console.ResetColor();
            Console.WriteLine("Create a new folder/template registry.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("(2) ");
            Console.ResetColor();
            Console.WriteLine("Update a folder/template registry.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("(3) ");
            Console.ResetColor();
            Console.WriteLine("Delete the data of the application.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("(4) ");
            Console.ResetColor();
            Console.WriteLine("Exit the application.\n");
            Console.Write("Your option: ");
            byte decision = 0;

            try
            {
                decision = Convert.ToByte(Console.ReadLine());
            }
            catch (FormatException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Only numbers are allowed.\n\n{e}");
                Console.ResetColor();

                Environment.Exit(1);
            }
            catch (OverflowException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Only numbers from 1 to 3 are accepted.\n\n{e}");
                Console.ResetColor();

                Environment.Exit(1);
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
                    DriveManagement.CreateFolderAndTemplate(folderName!, templateName!);

                    break;
                case 2:
                    Console.WriteLine("This is the list of the registries created:");

                    string[] registries = RegistryStorage.ReadFromRegistry();

                    int counter = 1;
                    foreach (string key in registries)
                    {
                        Console.WriteLine($"{counter}- {key}");
                        counter++;
                    }

                    break;
                case 3:
                    Console.WriteLine("All the data from the app will be deleted from the Windows registry.");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Are you sure? (y/n): ");
                    Console.ResetColor();
                    char deleteOrNot = Convert.ToChar(Console.Read());

                    if (Char.ToUpper(deleteOrNot) == 'Y')
                    {
                        RegistryStorage.DeleteAppData();
                        Console.WriteLine("All the data was deleted.");
                    }
                    else
                        Console.WriteLine("No data was deleted.");

                    break;
                case 4:
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Only \"1\", \"2\", \"3\" and \"4\" are valid options.");
                    Console.ResetColor();
                    break;
            }
        }
    }

    /// <summary>
    /// Class that contains the methods that make possible the connection with Google Drive,
    /// and that create and update the files.
    /// </summary>
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

        public static void CreateFolderAndTemplate(string folderName, string templateName)
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
                Name = $"{templateName}.xlsx",
                Parents = new List<string>
                {
                    folderId.Id
                }
            };

            Service.Files.Create(templateMetadata).Execute();
        }
    }

    /// <summary>
    /// Class that contains the methods that interact with the Windows Registry, saving,
    /// retrieving and deleting key/value pairs.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
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
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ManageFilesFromGoogleSheet")!;
            return key.GetValueNames();
        }
        public static string ReadFromRegistry(string registryPair)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ManageFilesFromGoogleSheet")!;
            return key.GetValue(registryPair)!.ToString()!;
        }

        public static void DeleteAppData()
        {
            Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\ManageFilesFromGoogleSheet");
        }
    }
}