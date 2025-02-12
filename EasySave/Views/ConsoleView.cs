using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Controllers;
using EasySave.Utils;
using EasySave.Logging;

namespace EasySave.Views
{
    /// <summary>
    /// Handles user interaction via the console.
    /// Provides a bilingual interface (French/English) with robust input validation.
    /// </summary>
    public class ConsoleView
    {
        private BackupController controller;
        private Dictionary<string, string> messages;

        public ConsoleView()
        {
            controller = new BackupController();
            // Default language is French.
            messages = Messages.French;
            SelectLanguage();
        }

        /// <summary>
        /// Prompts the user to select a language and sets the current messages.
        /// </summary>
        private void SelectLanguage()
        {
            Console.WriteLine(Messages.French["LanguagePrompt"]);
            Console.WriteLine(Messages.French["LanguageOption1"]);
            Console.WriteLine(Messages.French["LanguageOption2"]);
            Console.Write(Messages.French["LanguageChoice"]);
            string choice = Console.ReadLine().Trim();
            if (choice == "2")
                messages = Messages.English;
            else
                messages = Messages.French;

            // Update the global localization manager.
            LocalizationManager.CurrentMessages = messages;
        }

        /// <summary>
        /// Starts the interactive console interface.
        /// </summary>
        public void Start()
        {
            bool exit = false;
            while (!exit)
            {
                DisplayMenu();
                Console.Write(messages["PromptChoice"]);
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                            CreateBackupFlow();
                        break;
                    case "2":
                        controller.ListBackups();
                        break;
                    case "3":
                        if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                        {
                            Console.WriteLine(LocalizationManager.CurrentMessages["ExecutionNotAllowed"]);
                        }
                        else
                        {
                            if (controller.GetBackupCount() == 0)
                            {
                                Console.WriteLine(messages["NoBackupsToExecute"]);
                            }
                            else
                            {
                                controller.ListBackups();
                                ExecuteBackupFlow();
                            }
                        }
                        break;
                    case "4":
                        if (controller.GetBackupCount() == 0)
                        {
                            Console.WriteLine(messages["NoBackupsToDelete"]);
                        }
                        else
                        {
                            controller.ListBackups();
                            DeleteBackupFlow(); 
                        }
                        break;
                    case "5":
                        if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                        {
                            Console.WriteLine(LocalizationManager.CurrentMessages["ExecutionNotAllowed"]);
                        }
                        else
                        {
                            if (controller.GetBackupCount() ==0)
                            {
                                Console.WriteLine(messages["NoBackupsToExecute"]);
                            }
                            else
                            {
                                controller.ExecuteAllBackups();
                            } 
                        }
                        break;
                    case "6":
                        ConfigureBusinessSoftware();
                        break;
                    case "7":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine(messages["InvalidOption"]);
                        break;
                }
            }
        }

        /// <summary>
        /// Displays the main menu.
        /// </summary>
        private void DisplayMenu()
        {
            Console.WriteLine(messages["MenuTitle"]);
            Console.WriteLine(messages["MenuOption1"]);
            Console.WriteLine(messages["MenuOption2"]);
            Console.WriteLine(messages["MenuOption3"]);
            Console.WriteLine(messages["MenuOption4"]);
            Console.WriteLine(messages["MenuOption5"]);
            Console.WriteLine(messages["MenuOption6"]);
            Console.WriteLine(messages["MenuOption7"]);
        }

        /// <summary>
        /// Guides the user through the backup creation process.
        /// </summary>
        private void CreateBackupFlow()
        {
            Console.WriteLine(messages["BackupCreationTitle"]);

            string name = GetNonEmptyInput(messages["EnterBackupName"]);
            string source = GetValidSourcePath(messages["EnterSourcePath"]);
            string target = GetValidTargetPath(messages["EnterTargetPath"]);
            string typeStr = GetValidBackupType(messages["EnterBackupType"]);

            controller.CreateBackup(name, source, target, typeStr);
            Console.WriteLine(messages["BackupCreated"]);
        }

        /// <summary>
        /// Guides the user through the backup execution process.
        /// </summary>
        private void ExecuteBackupFlow()
        {
            Console.WriteLine(messages["BackupExecutionTitle"]);
            int idx = GetValidIndex(messages["EnterBackupIndex"]);
            controller.ExecuteBackup(idx - 1); // Convert from 1-indexed to 0-indexed.
        }

        /// <summary>
        /// Guides the user through the backup deletion process.
        /// </summary>
        private void DeleteBackupFlow()
        {
            Console.WriteLine(messages["BackupDeletionTitle"]);
            int idx = GetValidIndex(messages["EnterBackupIndex"]);
            controller.DeleteBackup(idx - 1);
        }
        
        /// <summary>
        /// Guides the user through the set up of the business software
        /// </summary>
        private void ConfigureBusinessSoftware()
        {
            Console.Clear();
            Console.WriteLine(LocalizationManager.CurrentMessages["ConfigureBusinessSoftwareTitle"]);
            Console.Write(LocalizationManager.CurrentMessages["EnterBusinessSoftwareName"]);
            string businessSoftwareName = Console.ReadLine().Trim();

            // Stocke le nom dans les paramètres généraux
            GeneralSettings.BusinessSoftwareName = businessSoftwareName;

            string confirmation = LocalizationManager.CurrentMessages["BusinessSoftwareConfigured"]
                .Replace("{name}", businessSoftwareName);
            Console.WriteLine(confirmation);
        }

        /// <summary>
        /// Prompts the user for a non-empty input.
        /// </summary>
        private string GetNonEmptyInput(string prompt)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine(messages["InvalidInput"]);
                }
            } while (string.IsNullOrWhiteSpace(input));
            return input.Trim();
        }

        /// <summary>
        /// Prompts the user for a valid source directory (which must exist).
        /// </summary>
        private string GetValidSourcePath(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string path = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine(messages["InvalidInput"]);
                    continue;
                }
                if (!Directory.Exists(path))
                {
                    Console.WriteLine(messages["SourceNotExist"]);
                    continue;
                }
                return path;
            }
        }

        /// <summary>
        /// Prompts the user for a valid target directory.
        /// If it does not exist, offers to create it.
        /// </summary>
        private string GetValidTargetPath(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string path = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine(messages["InvalidInput"]);
                    continue;
                }
                if (!Directory.Exists(path))
                {
                    Console.WriteLine(messages["TargetNotExist"]);
                    Console.Write(messages["CreateTargetPrompt"]);
                    string rep = Console.ReadLine().Trim().ToLower();
                    if (rep == "o" || rep == "oui" || rep == "y" || rep == "yes")
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                            Console.WriteLine(messages["TargetCreated"]);
                            return path;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine(messages["DifferentTargetPath"]);
                        continue;
                    }
                }
                return path;
            }
        }

        /// <summary>
        /// Prompts the user for a valid backup type ("full" or "diff").
        /// </summary>
        private string GetValidBackupType(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string type = Console.ReadLine().Trim().ToLower();
                if (string.IsNullOrWhiteSpace(type))
                {
                    Console.WriteLine(messages["InvalidInput"]);
                    continue;
                }
                if (type.StartsWith("f") || type.StartsWith("d"))
                {
                    return type;
                }
                else
                {
                    Console.WriteLine(messages["InvalidBackupType"]);
                }
            }
        }

        /// <summary>
        /// Prompts the user for a valid numeric index.
        /// </summary>
        private int GetValidIndex(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();
                if (int.TryParse(input, out int index))
                    return index;
                else
                    Console.WriteLine(messages["EnterValidNumber"]);
            }
        }
    }
}
