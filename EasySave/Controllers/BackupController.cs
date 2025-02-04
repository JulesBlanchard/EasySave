using System;
using EasySave.Models;

namespace EasySave.Controllers
{
    public class BackupController
    {
        // Création d'un logger + injection dans le BackupManager
        private IBackupLogger logger;
        private BackupManager manager;

        public BackupController()
        {
            // (à la racine du projet ou absolu)
            logger = new JsonBackupLogger("Logs");  
            manager = new BackupManager(logger);
        }

        /// <summary>
        /// Crée un backup en choisissant la stratégie, puis l'ajoute au manager
        /// </summary>
        public void CreateBackup(string name, string source, string target, string strategyType)
        {
            // On passe le type dans le constructeur
            var backup = new Backup(name, source, target, strategyType);
            if (strategyType.ToLower().StartsWith("f"))
            {
                backup.Strategy = new FullBackupStrategy();
            }
            else
            {
                backup.Strategy = new DifferentialBackupStrategy();
            }
            bool added = manager.AddBackup(backup);
            if (added)
            {
                Console.WriteLine($"[Controller] Backup '{name}' created with strategy={strategyType}.");
            }
        }


        /// <summary>
        /// Exécute un backup par son index
        /// </summary>
        public void ExecuteBackup(int index)
        {
            manager.ExecuteBackup(index);
        }
        
        public int GetBackupCount()
        {
            return manager.GetBackups().Count;
        }

        /// <summary>
        /// Liste tous les backups (avec leur stratégie)
        /// </summary>
        public void ListBackups()
        {
            var list = manager.GetBackups();
            Console.WriteLine("[Controller] Sauvegardes courantes:");
            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];
                // Affichage du nom et éventuellement du type de sauvegarde
                var strategyName = (b.Strategy == null) ? "None" : b.Strategy.GetType().Name;
                Console.WriteLine($"  {i + 1} - {b.Name} [Strategy={strategyName}] => Source={b.SourcePath}, Target={b.TargetPath}");
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
