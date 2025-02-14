using System;
using System.Collections.Generic;
using EasySave.Logging;
using EasySave.Models;
using EasySave.Utils;

namespace EasySave.Controllers
{
    /// <summary>
    /// Controller central pour gérer les sauvegardes.
    /// Implémenté en singleton pour que toutes les vues utilisent la même instance.
    /// </summary>
    public class BackupController
    {
        private IBackupLogger logger;
        private BackupManager manager;

        // Singleton
        public static BackupController Instance { get; } = new BackupController();

        // Événement notifiant du changement de la liste des sauvegardes
        public event Action BackupsChanged;

        // Constructeur privé
        public BackupController()
        {
            logger = LoggingManager.GetLogger("Logs");
            manager = new BackupManager(logger);
        }

        /// <summary>
        /// Crée une sauvegarde et la stocke dans le manager.
        /// </summary>
        public void CreateBackup(string name, string source, string target, string strategyType)
        {
            var backup = new Backup(name, source, target, strategyType);
            if (strategyType.ToLower().StartsWith("f"))
                backup.Strategy = new FullBackupStrategy();
            else
                backup.Strategy = new DifferentialBackupStrategy();

            manager.AddBackup(backup);
            OnBackupsChanged();

            string message = LocalizationManager.CurrentMessages["ControllerBackupCreated"];
            message = message.Replace("{name}", name).Replace("{strategy}", strategyType);
            // Vous pouvez logger ou afficher ce message selon vos besoins.
        }
        
        /// <summary>
        /// Permet de modifier une backup
        /// </summary>
        public void UpdateBackup(Backup backup)
        {
            // Recalculez la stratégie en fonction du type de sauvegarde
            if (!string.IsNullOrEmpty(backup.BackupType))
            {
                if (backup.BackupType.ToLower().StartsWith("f"))
                    backup.Strategy = new FullBackupStrategy();
                else
                    backup.Strategy = new DifferentialBackupStrategy();
            }

            // Enregistrer la liste des sauvegardes mise à jour
            manager.UpdateBackupsFile();
            OnBackupsChanged();
        }


        /// <summary>
        /// Supprime une sauvegarde par index.
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

        // Méthode privée pour notifier les abonnés
        private void OnBackupsChanged()
        {
            BackupsChanged?.Invoke();
        }
    }
}
