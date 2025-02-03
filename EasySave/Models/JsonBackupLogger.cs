namespace EasySave.Models
{
    public class JsonBackupLogger : IBackupLogger
    {
        // Par exemple, on peut stocker un chemin de fichier log
        private string dailyLogFilePath;

        public JsonBackupLogger(string logDirectory)
        {
            // TODO: Construire un chemin style "2025-xx-xx-Log.json" 
            // ou juste stocker "logDirectory"
            this.dailyLogFilePath = System.IO.Path.Combine(
                logDirectory, "Log.json"
            );
        }

        public void LogTransfer(string backupName, string sourceFile, string destFile, long fileSize, long transferTimeMs)
        {
            // TODO: Écriture d'une entrée JSON de transfert réussi dans dailyLogFilePath
        }

        public void LogError(string backupName, string sourceFile, string destFile, System.Exception ex)
        {
            // TODO: Écriture d'une entrée JSON pour logguer l'erreur
        }
    }
}