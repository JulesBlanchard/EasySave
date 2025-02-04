using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Models
{
    public class BackupManager
    {
        private const int MAX_BACKUPS = 5;
        private List<Backup> backups = new List<Backup>();
        private IBackupLogger logger;
        // Chemin vers le fichier de persistance des backups
        private readonly string backupFilePath;

        public BackupManager(IBackupLogger logger)
        {
            this.logger = logger;
            // On enregistre le fichier backups.json dans le répertoire d'exécution
            backupFilePath = Path.Combine(AppContext.BaseDirectory, "backups.json");
            LoadBackups();
        }

        /// <summary>
        /// Ajoute un backup et sauvegarde la liste sur le disque
        /// </summary>
        public bool AddBackup(Backup backup)
        {
            if (backups.Count >= MAX_BACKUPS)
            {
                Console.WriteLine("[BackupManager] Impossible d'ajouter plus de 5 backups.");
                return false;
            }
            backups.Add(backup);
            SaveBackups();
            Console.WriteLine($"[BackupManager] Backup '{backup.Name}' ajouté. (count={backups.Count})");
            return true;
        }

        /// <summary>
        /// Exécute un backup par son index
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
            backup.Execute(logger);
        }

        /// <summary>
        /// Exécute tous les backups séquentiellement
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
        /// Retourne la liste des backups
        /// </summary>
        public List<Backup> GetBackups()
        {
            return backups;
        }

        /// <summary>
        /// Sauvegarde la liste des backups dans backups.json
        /// </summary>
        private void SaveBackups()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(backups, options);
                File.WriteAllText(backupFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[BackupManager] Erreur lors de la sauvegarde des backups : " + ex.Message);
            }
        }

        /// <summary>
        /// Charge la liste des backups depuis backups.json s'il existe
        /// </summary>
        private void LoadBackups()
        {
            if (File.Exists(backupFilePath))
            {
                try
                {
                    string json = File.ReadAllText(backupFilePath);
                    List<Backup> loadedBackups = JsonSerializer.Deserialize<List<Backup>>(json);
                    if (loadedBackups != null)
                    {
                        backups = loadedBackups;
                        // Pour chaque backup, recréer la stratégie en fonction du BackupType
                        foreach (var backup in backups)
                        {
                            if (!string.IsNullOrEmpty(backup.BackupType))
                            {
                                if (backup.BackupType.ToLower().StartsWith("f"))
                                    backup.Strategy = new FullBackupStrategy();
                                else
                                    backup.Strategy = new DifferentialBackupStrategy();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[BackupManager] Erreur lors du chargement des backups : " + ex.Message);
                }
            }
        }

    }
}
