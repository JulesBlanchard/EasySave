namespace EasySave.Models
{
    public interface IBackupLogger
    {
        /// <summary>
        /// Loggue un transfert de fichier (réussi).
        /// </summary>
        void LogTransfer(
            string backupName,
            string sourceFile,
            string destFile,
            long fileSize,
            long transferTimeMs
        );

        /// <summary>
        /// Loggue une erreur lors d'un transfert (exception).
        /// </summary>
        void LogError(
            string backupName,
            string sourceFile,
            string destFile,
            System.Exception ex
        );
    }
}