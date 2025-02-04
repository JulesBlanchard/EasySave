using System;
using System.Collections.Generic;

namespace EasySave.Models
{
    public class BackupManager
    {
        // Contient jusqu’à 5 backups
        private List<Backup> backups = new List<Backup>();

        // Le Manager a besoin d'un logger
        private IBackupLogger logger;

        public BackupManager(IBackupLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Ajoute un backup si on n’a pas dépassé le max (5).
        /// </summary>
        public bool AddBackup(Backup backup)
        {
            backups.Add(backup);
            Console.WriteLine($"[BackupManager] Backup '{backup.Name}' ajouté. (count={backups.Count})");
            return true;
        }

        /// <summary>
        /// Exécute un backup par son index (0..n).
        /// </summary>
        public void ExecuteBackup(int index)
        {
            if (index < 0 || index >= backups.Count)
            {
                Console.WriteLine("[BackupManager] Index invalide.");
                return;
            }
            var backup = backups[index];
            Console.WriteLine($"[BackupManager] Exécution du backup '{backup.Name}' (index={index})");
            
            // On appelle backup.Execute(logger) pour logguer
            backup.Execute(logger);
        }

        /// <summary>
        /// Exécute tous les backups séquentiellement.
        /// </summary>
        public void ExecuteAll()
        {
            if (backups.Count == 0)
            {
                Console.WriteLine("[BackupManager] Aucun backup à exécuter.");
                return;
            }
            Console.WriteLine($"[BackupManager] Exécution de {backups.Count} backup(s)...");
            for (int i = 0; i < backups.Count; i++)
            {
                ExecuteBackup(i);
            }
        }

        /// <summary>
        /// Permet d'inspecter la liste de backups
        /// </summary>
        public List<Backup> GetBackups()
        {
            return backups;
        }
    }
}
