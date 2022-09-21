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

        /// <summary>
        /// Prints the menu of the application, which includes the 4 options available.
        /// Also, it calls the methods responsible for executing each command, both
        /// regarding the Google Drive API and the Windows Registry.
        /// </summary>
        /// <exception cref="FormatException">
        /// If the user enters a character that is not a number.
        /// </exception>
        /// <exception cref="OverflowException">
        /// If the user enters a number that is beyond the maximum or minimum value of
        /// the <c>byte</c> data type.
        /// </exception>
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
                Console.WriteLine($"Only numbers from 1 to 4 are accepted.\n\n{e}");
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
        static string ApplicationName = "ManageFilesFromGoogleSheet";
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
                Console.WriteLine($"The credentials file was not found.\n\n{e}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Create the folder and the template on the user's Google Drive account
        /// based on the names provided.
        /// </summary>
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

        /// <summary>
        /// Update both the folder (creating or deleting files from the folder) and
        /// the Google Sheet file, taking information from it to update the folder and
        /// changing rows and columns in the file according to the information
        /// retrieved from the files of the folder.
        /// </summary>
        public static void UpdateFolderAndTemplate()
        {

        }
    }

    /// <summary>
    /// Class that contains the methods that interact with the Windows Registry, saving,
    /// retrieving and deleting key/value pairs.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class RegistryStorage
    {
        /// <summary>
        /// Saves the information of the folder/template pair into the Windows Registry,
        /// so the user doesn't need to provide the information again but only select the
        /// number corresponding to the pair they want to update.
        /// </summary>
        /// <param name="folderName">Name of the folder to contain all the Google files.</param>
        /// <param name="templateName">Name of the Google Sheet file to use as a template.</param>
        public static void SaveToRegistry(string folderName, string templateName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ManageFilesFromGoogleSheet");
            key.SetValue(folderName, templateName);
            key.Close();
        }
        
        /// <summary>
        /// Retrieves to other methods all of the folder names so the user can select
        /// which one they want to update.
        /// </summary>
        /// <returns>
        /// An array of strings with all the folder/template pairs.
        /// </returns>
        public static string[] ReadFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ManageFilesFromGoogleSheet")!;
            return key.GetValueNames();
        }

        /// <summary>
        /// Retrieves to other methods the value of the specified key.
        /// </summary>
        /// <param name="registryPair">Name of the folder/template pair to retrieve.</param>
        /// <returns>
        /// The value of the specified key.
        /// </returns>
        public static string ReadFromRegistry(string registryPair)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ManageFilesFromGoogleSheet")!;
            return key.GetValue(registryPair)!.ToString()!;
        }

        /// <summary>
        /// Deletes all the information from the <c>SOFTWARE\ManageFilesFromGoogleSheet</c>
        /// directory inside the HKEY_CURRENT_USER from the Windows registry. It deletes the
        //whole folder.
        /// </summary>
        public static void DeleteAppData()
        {
            Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\ManageFilesFromGoogleSheet");
        }
    }
}