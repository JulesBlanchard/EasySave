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

        public void CreateBackup(string name, string source, string target)
        {
            var backup = new Backup(name, source, target);
            backups.Add(backup);
            Console.WriteLine($"[Controller] Backup '{name}' created.");
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
                Console.WriteLine($"  {i} - {backups[i].Name} (source={backups[i].SourcePath}, target={backups[i].TargetPath})");
            }
        }
    }
}