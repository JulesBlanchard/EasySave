using System;
using System.IO;
using EasySave.Controllers;
using EasySave.Models;

namespace EasySave.Views
{
    public class ConsoleView
    {
        private BackupController controller;

        public ConsoleView()
        {
            controller = new BackupController();
        }

        public void Start()
        {
            bool exit = false;
            while (!exit)
            {
                DisplayMenu();
                Console.Write("Choix : ");
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        if (controller.GetBackupCount() >= 5)
                        {
                            Console.WriteLine("Impossible de créer plus de 5 backups.");
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
                        Console.WriteLine("Option inconnue.");
                        break;
                }
            }
        }

        private void DisplayMenu()
        {
            Console.WriteLine("=== EasySave ===");
            Console.WriteLine("1. Créer un backup");
            Console.WriteLine("2. Lister les backups");
            Console.WriteLine("3. Exécuter un backup");
            Console.WriteLine("4. Quitter");
        }

        private void CreateBackupFlow()
        {
            Console.WriteLine("=== Création d'un Backup ===");

            // Saisie du nom du backup (doit être non vide)
            string name = GetNonEmptyInput("Nom du backup : ");

            // Saisie et vérification du chemin source (le répertoire doit exister)
            string source = GetValidSourcePath("Chemin source : ");

            // Saisie et vérification du chemin cible (s'il n'existe pas, possibilité de le créer)
            string target = GetValidTargetPath("Chemin cible : ");

            // Saisie du type de sauvegarde (full/diff)
            string typeStr = GetValidBackupType("Type de sauvegarde (full/diff) : ");

            // Appel à la méthode CreateBackup du contrôleur qui prend en compte la stratégie
            controller.CreateBackup(name, source, target, typeStr);

            Console.WriteLine("Backup créé avec succès !");
        }

        private void ExecuteBackupFlow()
        {
            Console.WriteLine("=== Exécution d'un Backup ===");
            int idx = GetValidIndex("Indice du backup à exécuter : ");
            // Dans votre code initial, on décrémente de 1 pour correspondre à l'indexation (si nécessaire)
            controller.ExecuteBackup(idx - 1);
        }

        /// <summary>
        /// Demande à l'utilisateur une saisie non vide.
        /// </summary>
        /// <param name="prompt">Le message à afficher.</param>
        /// <returns>La saisie de l'utilisateur non vide.</returns>
        private string GetNonEmptyInput(string prompt)
        {
            string input = "";
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("L'entrée ne peut pas être vide. Veuillez réessayer.");
                }
            } while (string.IsNullOrWhiteSpace(input));
            return input.Trim();
        }

        /// <summary>
        /// Demande à l'utilisateur de saisir un chemin source valide (le répertoire doit exister).
        /// </summary>
        /// <param name="prompt">Le message à afficher.</param>
        /// <returns>Un chemin source valide.</returns>
        private string GetValidSourcePath(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string path = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("Le chemin source ne peut pas être vide.");
                    continue;
                }
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Le répertoire source n'existe pas. Veuillez vérifier le chemin.");
                    continue;
                }
                return path;
            }
        }

        /// <summary>
        /// Demande à l'utilisateur de saisir un chemin cible valide.
        /// Si le répertoire n'existe pas, il propose de le créer.
        /// </summary>
        /// <param name="prompt">Le message à afficher.</param>
        /// <returns>Un chemin cible valide.</returns>
        private string GetValidTargetPath(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string path = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("Le chemin cible ne peut pas être vide.");
                    continue;
                }
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Le répertoire cible n'existe pas.");
                    Console.Write("Voulez-vous le créer ? (O/N) : ");
                    string rep = Console.ReadLine().Trim().ToLower();
                    if (rep == "o" || rep == "oui")
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                            Console.WriteLine("Répertoire créé.");
                            return path;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Erreur lors de la création du répertoire : " + ex.Message);
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
        /// <param name="prompt">Le message à afficher.</param>
        /// <returns>Le type de sauvegarde validé.</returns>
        private string GetValidBackupType(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string type = Console.ReadLine().Trim().ToLower();
                if (string.IsNullOrWhiteSpace(type))
                {
                    Console.WriteLine("Le type de sauvegarde ne peut pas être vide.");
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
        /// <param name="prompt">Le message à afficher.</param>
        /// <returns>L'indice validé.</returns>
        private int GetValidIndex(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();
                if (int.TryParse(input, out int index))
                    return index;
                else
                    Console.WriteLine("Veuillez entrer un nombre valide.");
            }
        }
    }
}
