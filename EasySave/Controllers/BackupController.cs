using System;
using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.Controllers
{
    public class BackupController
    {
        // Liste de backups 
        // On implémentera "BackupManager" dans une autre feature.
        private List<Backup> backups = new List<Backup>();

        public void CreateBackup(string name, string source, string target, string strategyType)
        {
            var backup = new Backup(name, source, target);

            // Choix de la stratégie
            if (strategyType.ToLower().StartsWith("f")) // "full"
            {
                backup.Strategy = new FullBackupStrategy();
            }
            else
            {
                backup.Strategy = new DifferentialBackupStrategy();
            }

            backups.Add(backup);
            Console.WriteLine($"[Controller] Backup '{name}' created with strategy={strategyType}.");
        }
        public void CreateBackup(string name, string source, string target)
        {
            var backup = new Backup(name, source, target);
            backup.Strategy = new FullBackupStrategy();
            backups.Add(backup);
            Console.WriteLine($"[Controller] Backup '{name}' created (default FULL).");
        }

        public void ExecuteBackup(int index)
        {
            if (index >= 0 && index < backups.Count)
            {
                var backup = backups[index];
                Console.WriteLine($"[Controller] Executing backup '{backup.Name}'...");
                backup.Execute();
            }
            else
            {
                Console.WriteLine("[Controller] Invalid backup index.");
            }
        }

        public void ListBackups()
        {
            Console.WriteLine("[Controller] Current backups:");
            for (int i = 0; i < backups.Count; i++)
            {
                var b = backups[i];
                // On peut deviner le type de strategy
                var strategyName = (b.Strategy == null) ? "None" : b.Strategy.GetType().Name;
                Console.WriteLine($"  {i} - {b.Name} [Strategy={strategyName}] => Source={b.SourcePath}, Target={b.TargetPath}");
            }
        }
    }
}