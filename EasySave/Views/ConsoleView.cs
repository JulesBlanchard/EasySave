using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Controllers;
using EasySave.Utils;

namespace EasySave.Views
{
    public class ConsoleView
    {
        private BackupController controller;
        private Dictionary<string, string> messages;

        public ConsoleView()
        {
            controller = new BackupController();
            // Par défaut, on prend le français
            messages = Messages.French;
            SelectLanguage();
        }

        /// <summary>
        /// Permet à l'utilisateur de choisir la langue.
        /// </summary>
        private void SelectLanguage()
        {
            Console.WriteLine(Messages.French["LanguagePrompt"]);
            Console.WriteLine(Messages.French["LanguageOption1"]);
            Console.WriteLine(Messages.French["LanguageOption2"]);
            Console.Write(Messages.French["LanguageChoice"]);
            string choice = Console.ReadLine().Trim();
            // Si l'utilisateur choisit "2", on prend l'anglais, sinon le français.
            if (choice == "2")
                messages = Messages.English;
            else
                messages = Messages.French;
        }

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
                        if (controller.GetBackupCount() >= 5)
                        {
                            Console.WriteLine(messages["CannotCreateMoreBackups"]);
                        }
                        else
                        {
                            CreateBackupFlow();
                        }
                        break;
                    case "2":
                        controller.ListBackups();
                        break;
                    case "3":
                        controller.ListBackups();
                        ExecuteBackupFlow();
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine(messages["InvalidOption"]);
                        break;
                }
            }
        }

        private void DisplayMenu()
        {
            Console.WriteLine(messages["MenuTitle"]);
            Console.WriteLine(messages["MenuOption1"]);
            Console.WriteLine(messages["MenuOption2"]);
            Console.WriteLine(messages["MenuOption3"]);
            Console.WriteLine(messages["MenuOption4"]);
        }

        private void CreateBackupFlow()
        {
            Console.WriteLine(messages["BackupCreationTitle"]);

            // Demander le nom du backup
            string name = GetNonEmptyInput(messages["EnterBackupName"]);
            // Demander et vérifier le chemin source
            string source = GetValidSourcePath(messages["EnterSourcePath"]);
            // Demander et vérifier le chemin cible
            string target = GetValidTargetPath(messages["EnterTargetPath"]);
            // Demander le type de sauvegarde
            string typeStr = GetValidBackupType(messages["EnterBackupType"]);

            controller.CreateBackup(name, source, target, typeStr);

            Console.WriteLine(messages["BackupCreated"]);
        }

        private void ExecuteBackupFlow()
        {
            Console.WriteLine(messages["BackupExecutionTitle"]);
            int idx = GetValidIndex(messages["EnterBackupIndex"]);
            // On décrémente de 1 si l'index est 1-indexé.
            controller.ExecuteBackup(idx - 1);
        }

        /// <summary>
        /// Demande à l'utilisateur une saisie non vide.
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
        /// Demande à l'utilisateur un chemin source valide (le répertoire doit exister).
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
        /// Demande à l'utilisateur un chemin cible valide.
        /// Si le répertoire n'existe pas, propose de le créer.
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
                            Console.WriteLine("Erreur : " + ex.Message);
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Veuillez saisir un autre chemin cible.");
                        continue;
                    }
                }
                return path;
            }
        }

        /// <summary>
        /// Demande à l'utilisateur de saisir un type de sauvegarde valide ("full" ou "diff").
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
                    Console.WriteLine("Type invalide. Veuillez saisir 'full' ou 'diff'.");
                }
            }
        }

        /// <summary>
        /// Demande à l'utilisateur de saisir un indice numérique valide.
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
