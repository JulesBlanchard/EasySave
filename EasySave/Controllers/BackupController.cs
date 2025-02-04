using System;
using EasySave.Models;

namespace EasySave.Controllers
{
    public class BackupController
    {
        // Au lieu d'une List<Backup>, on a désormais un BackupManager
        private BackupManager manager = new BackupManager();

        /// <summary>
        /// Crée un backup en choisissant la stratégie, puis l'ajoute au manager
        /// </summary>
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

            // On l'ajoute au manager
            bool added = manager.AddBackup(backup);
            if (added)
            {
                Console.WriteLine($"[Controller] Backup '{name}' created with strategy={strategyType}.");
            }
        }

        /// <summary>
        /// Surcharge si on veut un backup par défaut full
        /// </summary>
        public void CreateBackup(string name, string source, string target)
        {
            var backup = new Backup(name, source, target);
            backup.Strategy = new FullBackupStrategy();
            bool added = manager.AddBackup(backup);
            if (added)
            {
                Console.WriteLine($"[Controller] Backup '{name}' created (default FULL).");
            }
        }

        /// <summary>
        /// Exécute un backup par son index
        /// </summary>
        public void ExecuteBackup(int index)
        {
            manager.ExecuteBackup(index);
        }

        /// <summary>
        /// Liste tous les backups (avec leur stratégie)
        /// </summary>
        public void ListBackups()
        {
            var backups = manager.GetBackups();
            Console.WriteLine("[Controller] Current backups:");
            for (int i = 0; i < backups.Count; i++)
            {
                var b = backups[i];
                var strategyName = (b.Strategy == null) ? "None" : b.Strategy.GetType().Name;
                Console.WriteLine($"  {i} - {b.Name} [Strategy={strategyName}] => Source={b.SourcePath}, Target={b.TargetPath}");
            }
        }

        /// <summary>
        /// Exécute tous les backups
        /// </summary>
        public void ExecuteAllBackups()
        {
            manager.ExecuteAll();
        }
    }
}
