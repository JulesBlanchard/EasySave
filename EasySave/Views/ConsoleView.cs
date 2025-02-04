using System;
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
            while (true)
            {
                DisplayMenu();
                Console.Write("Choix : ");
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        if (controller.GetBackupCount() == 5)
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
                        return;
                    default:
                        Console.WriteLine("Option inconnue.");
                        break;
                }
            }
        }

        private void DisplayMenu()
        {
            Console.WriteLine("=== EasySave - Feature: Strategy ===");
            Console.WriteLine("1. Créer un backup");
            Console.WriteLine("2. Lister les backups");
            Console.WriteLine("3. Exécuter un backup");
            Console.WriteLine("4. Quitter");
        }

        private void CreateBackupFlow()
        {
            Console.Write("Nom du backup: ");
            var name = Console.ReadLine();

            Console.Write("Chemin source: ");
            var source = Console.ReadLine();

            Console.Write("Chemin cible: ");
            var target = Console.ReadLine();

            Console.Write("Type de sauvegarde (full/diff)? ");
            var typeStr = Console.ReadLine();

            // On appelle la méthode "CreateBackup" qui prend en compte la stratégie
            controller.CreateBackup(name, source, target, typeStr);
        }

        private void ExecuteBackupFlow()
        {
            Console.Write("Indice du backup à exécuter : ");
            if (int.TryParse(Console.ReadLine(), out int idx))
            {
                controller.ExecuteBackup(idx - 1);
            }
            else
            {
                Console.WriteLine("Indice invalide.");
            }
        }
    }
}
