using System;
using System.Collections.Generic;

namespace EasySave.Models
{
    public class BackupManager
    {
        private const int MAX_BACKUPS = 5;

        // Contient jusqu’à 5 backups
        private List<Backup> backups = new List<Backup>();

        /// <summary>
        /// Ajoute un backup si on n’a pas dépassé le max (5).
        /// </summary>
        public bool AddBackup(Backup backup)
        {
            if (backups.Count >= MAX_BACKUPS)
            {
                Console.WriteLine("[BackupManager] Impossible d'ajouter plus de 5 backups.");
                return false;
            }
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
            backup.Execute();
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
