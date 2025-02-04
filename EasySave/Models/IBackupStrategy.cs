namespace EasySave.Models
{
    public interface IBackupStrategy
    {
        /// <summary>
        /// Exécute la sauvegarde pour le Backup spécifié, en logguant via IBackupLogger.
        /// </summary>
        void Execute(Backup backup, IBackupLogger logger);
    }
}