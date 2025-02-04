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
            // Ici, le chemin peut être relatif ou construit à partir du répertoire d'exécution.
            this.logDirectory = Path.Combine(AppContext.BaseDirectory, logDirectory);
            Directory.CreateDirectory(this.logDirectory);
        }

        public void LogTransfer(string backupName, string sourceFile, string destFile, long fileSize, long transferTimeMs)
        {
            var entry = new
            {
                Name = backupName,
                FileSource = sourceFile,
                FileTarget = destFile,
                FileSize = fileSize,
                FileSizeUnit = "octets",            // Unité pour la taille
                FileTransferTime = transferTimeMs,
                FileTransferTimeUnit = "ms",         // Unité pour le temps de transfert
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            WriteLogEntry(entry);
        }


        public void LogError(string backupName, string sourceFile, string destFile, Exception ex)
        {
            var entry = new
            {
                Name = backupName,
                FileSource = sourceFile,
                FileTarget = destFile,
                FileSize = 0,
                FileSizeUnit = "octets",            // Unité pour la taille (même en cas d'erreur)
                FileTransferTime = -1,              // Pour signaler une erreur, par exemple -1
                FileTransferTimeUnit = "ms",        // Unité pour le temps
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Error = ex.Message
            };

            WriteLogEntry(entry);
        }


        private void WriteLogEntry(object logEntry)
        {
            // Nom du fichier journalier au format "yyyy-MM-dd-Log.json"
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-Log.json";
            string filePath = Path.Combine(logDirectory, fileName);

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                // Sérialisation de l'objet log
                string jsonLine = JsonSerializer.Serialize(logEntry, options);
                // On ajoute la ligne au fichier (chaque objet sur sa propre ligne)
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine(jsonLine);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[JsonBackupLogger] Erreur lors de l'écriture du log : " + e.Message);
            }
        }
    }
}
