using System;
using EasySave.Models;
using EasySave.Logging;
using EasySave.Utils;

namespace EasySave.Controllers
{
    /// <summary>
    /// Controller class that orchestrates backup creation, listing, and execution.
    /// </summary>
    public class BackupController
    {
        private IBackupLogger logger;
        private BackupManager manager;

        public BackupController()
        {
            logger = new JsonBackupLogger("Logs");
            manager = new BackupManager(logger);
        }

        /// <summary>
        /// Creates a backup with the chosen strategy and adds it to the manager.
        /// </summary>
        public void CreateBackup(string name, string source, string target, string strategyType)
        {
            var backup = new Backup(name, source, target, strategyType);
            if (strategyType.ToLower().StartsWith("f"))
                backup.Strategy = new FullBackupStrategy();
            else
                backup.Strategy = new DifferentialBackupStrategy();

            bool added = manager.AddBackup(backup);
            if (added)
            {
                string message = LocalizationManager.CurrentMessages["ControllerBackupCreated"];
                message = message.Replace("{name}", name).Replace("{strategy}", strategyType);
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Executes a backup by its index.
        /// </summary>
        public void ExecuteBackup(int index)
        {
            manager.ExecuteBackup(index);
        }
        
        /// <summary>
        /// Deletes a backup by its index.
        /// </summary>
        public void DeleteBackup(int index)
        {
            manager.DeleteBackup(index);
        }
        

        /// <summary>
        /// Returns the current number of backups.
        /// </summary>
        public int GetBackupCount()
        {
            return manager.GetBackups().Count;
        }

        /// <summary>
        /// Lists all backups.
        /// </summary>
        public void ListBackups()
        {
            var list = manager.GetBackups();
            Console.WriteLine(LocalizationManager.CurrentMessages["ListBackupsTitle"]);
            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];
                string strategyName = (b.Strategy == null) ? "None" : b.Strategy.GetType().Name;
                Console.WriteLine($"  {i + 1} - {b.Name} [Strategy={strategyName}] => Source={b.SourcePath}, Target={b.TargetPath}");
            }
        }

        /// <summary>
        /// Executes all backups sequentially.
        /// </summary>
        public void ExecuteAllBackups()
        {
            manager.ExecuteAll();
        }
    }
}
