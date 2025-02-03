using System;
using EasySave.Controllers;

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
                        CreateBackupFlow();
                        break;
                    case "2":
                        controller.ListBackups();
                        break;
                    case "3":
                        Console.Write("Indice du backup à exécuter : ");
                        if (int.TryParse(Console.ReadLine(), out int idx))
                        {
                            controller.ExecuteBackup(idx);
                        }
                        else
                        {
                            Console.WriteLine("Indice invalide.");
                        }
                        break;
                    case "4":
                        return; // Quitter
                    default:
                        Console.WriteLine("Option inconnue.");
                        break;
                }
            }
        }

        private void DisplayMenu()
        {
            Console.WriteLine("=== EasySave (Feature: Implement Backup) ===");
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

            controller.CreateBackup(name, source, target);
        }
    }
}
