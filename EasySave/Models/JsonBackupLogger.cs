using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Models
{
    public class JsonBackupLogger : IBackupLogger
    {
        private string logDirectory;

        public JsonBackupLogger(string logDirectory)
        {
            this.logDirectory = logDirectory;

            // S'assurer que le dossier existe
            Directory.CreateDirectory(logDirectory);
        }

        public void LogTransfer(string backupName, string sourceFile, string destFile, long fileSize, long transferTimeMs)
        {
            var entry = new
            {
                Timestamp = DateTime.Now.ToString("O"),    // format ISO8601
                EventType = "Transfer",
                BackupName = backupName,
                SourceFile = sourceFile,
                DestFile = destFile,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs
            };

            WriteLogEntry(entry);
        }

        public void LogError(string backupName, string sourceFile, string destFile, Exception ex)
        {
            var entry = new
            {
                Timestamp = DateTime.Now.ToString("O"),
                EventType = "Error",
                BackupName = backupName,
                SourceFile = sourceFile,
                DestFile = destFile,
                ErrorMessage = ex.Message,
                TransferTimeMs = -1
            };

            WriteLogEntry(entry);
        }

        private void WriteLogEntry(object logEntry)
        {
            // Nom de fichier style "2023-01-01-Log.json"
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-Log.json";
            string filePath = Path.Combine(logDirectory, fileName);

            try
            {
                // Sérialiser en JSON
                string jsonLine = JsonSerializer.Serialize(logEntry);

                // Ajouter la ligne dans le fichier
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine(jsonLine);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[JsonBackupLogger] Failed to write log: " + e.Message);
            }
        }
    }
}
