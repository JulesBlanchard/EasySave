using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Logging;
using EasySave.Utils;

namespace EasySave.Models
{
    /// <summary>
    /// Manages the collection of backups. Limits to 5 backups, persists them in backups.json, and orchestrates execution.
    /// </summary>
    public class BackupManager
    {
        private List<Backup> backups = new List<Backup>();
        private IBackupLogger logger;
        private readonly string backupFilePath;

        public BackupManager(IBackupLogger logger)
        {
            this.logger = logger;
            backupFilePath = Path.Combine(AppContext.BaseDirectory, "backups.json");
            LoadBackups();
        }
        
        public void UpdateBackupsFile()
        {
            SaveBackups();
        }

        /// <summary>
        /// Adds a backup and saves the list to disk.
        /// </summary>
        public void AddBackup(Backup backup)
        {
            backups.Add(backup);
            SaveBackups();
            string msg = LocalizationManager.CurrentMessages["ControllerBackupAdded"];
            msg = msg.Replace("{name}", backup.Name).Replace("{count}", backups.Count.ToString());
            Console.WriteLine(msg);
        }
        
        /// <summary>
        /// Executes a backup by its index.
        /// </summary>
        public void DeleteBackup(int index)
        {
            if (index < 0 || index >= backups.Count)
            {
                Console.WriteLine(LocalizationManager.CurrentMessages["ControllerInvalidIndex"]);
                return;
            }
            backups.RemoveAt(index);
            SaveBackups();
            string deleteMsg = LocalizationManager.CurrentMessages["ControllerDeletingBackup"];
            deleteMsg = deleteMsg.Replace("{index}", (index + 1).ToString());
            Console.WriteLine(deleteMsg);
        }



        /// <summary>
        /// Executes a backup by its index.
        /// </summary>
        public void ExecuteBackup(int index)
        {
            if (index < 0 || index >= backups.Count)
            {
                Console.WriteLine(LocalizationManager.CurrentMessages["ControllerInvalidIndex"]);
                return;
            }
            var backup = backups[index];
            string execMsg = LocalizationManager.CurrentMessages["ControllerExecutingBackup"];
            execMsg = execMsg.Replace("{name}", backup.Name).Replace("{index}", index.ToString());
            Console.WriteLine(execMsg);
            backup.Execute(logger);
        }

        /// <summary>
        /// Executes all backups sequentially.
        /// </summary>
        public void ExecuteAll()
        {
            if (backups.Count == 0)
            {
                Console.WriteLine(LocalizationManager.CurrentMessages["NoBackupsToExecute"]);
                return;
            }
            string execAllMsg = LocalizationManager.CurrentMessages["ControllerExecutingAll"];
            execAllMsg = execAllMsg.Replace("{count}", backups.Count.ToString());
            Console.WriteLine(execAllMsg);
            for (int i = 0; i < backups.Count; i++)
            {
                ExecuteBackup(i);
            }
        }

        /// <summary>
        /// Returns the list of backups.
        /// </summary>
        public List<Backup> GetBackups()
        {
            return backups;
        }

        /// <summary>
        /// Saves the list of backups to backups.json.
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
                Console.WriteLine("[BackupManager] Error saving backups: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads the list of backups from backups.json if it exists.
        /// </summary>
        private void LoadBackups()
        {
            if (File.Exists(backupFilePath))
            {
                try
                {
                    string json = File.ReadAllText(backupFilePath);
                    // Check if the file is empty or contains only whitespace.
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        backups = new List<Backup>();
                        return;
                    }
                    List<Backup> loadedBackups = JsonSerializer.Deserialize<List<Backup>>(json);
                    if (loadedBackups != null)
                    {
                        backups = loadedBackups;
                        // For each backup, recreate the strategy based on its BackupType.
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
                    Console.WriteLine(LocalizationManager.CurrentMessages["BackupManager_LoadError"].Replace("{error}", ex.Message));
                }
            }
        }
    }
}
