using System;
using System.Collections.Generic;
using EasySave.Logging;
using EasySave.Models;
using EasySave.Utils;

namespace EasySave.Controllers
{
    /// <summary>
    /// Central controller for managing backups.
    /// Implemented as a singleton so that all views use the same instance.
    /// </summary>
    public class BackupController
    {
        private IBackupLogger logger;
        private BackupManager manager;

        // Singleton
        public static BackupController Instance { get; } = new BackupController();

        // Event notifying when the backup list changes
        public event Action BackupsChanged;

        // Private constructor
        public BackupController()
        {
            logger = LoggingManager.GetLogger("Logs");
            manager = new BackupManager(logger);
        }

        /// <summary>
        /// Creates a backup and stores it in the manager.
        /// </summary>
        public void CreateBackup(string name, string source, string target, string strategyType, bool shouldEncrypt, string encryptionKey)
        {
            var backup = new Backup(name, source, target, strategyType)
            {
                ShouldEncrypt = shouldEncrypt,
                EncryptionKey = encryptionKey
            };
    
            if (strategyType.ToLower().StartsWith("f"))
                backup.Strategy = new FullBackupStrategy();
            else
                backup.Strategy = new DifferentialBackupStrategy();

            manager.AddBackup(backup);
            OnBackupsChanged();

            string message = LocalizationManager.CurrentMessages["ControllerBackupCreated"];
            message = message.Replace("{name}", name).Replace("{strategy}", strategyType);
        }

        
        /// <summary>
        /// Allows modifying a backup.
        /// </summary>
        public void UpdateBackup(Backup backup)
        {
            // Recalculate the strategy based on the backup type
            if (!string.IsNullOrEmpty(backup.BackupType))
            {
                if (backup.BackupType.ToLower().StartsWith("f"))
                    backup.Strategy = new FullBackupStrategy();
                else
                    backup.Strategy = new DifferentialBackupStrategy();
            }

            // Save the updated list of backups
            manager.UpdateBackupsFile();
            OnBackupsChanged();
        }


        /// <summary>
        /// Deletes a backup by index.
        /// </summary>
        public void DeleteBackup(int index)
        {
            manager.DeleteBackup(index);
            OnBackupsChanged();
        }

        public void ExecuteBackup(int index)
        {
            manager.ExecuteBackup(index);
        }

        public void ExecuteAllBackups()
        {
            manager.ExecuteAll();
        }

        public List<Backup> GetBackups()
        {
            return manager.GetBackups();
        }

        // Private method to notify subscribers
        private void OnBackupsChanged()
        {
            BackupsChanged?.Invoke();
        }
    }
}
