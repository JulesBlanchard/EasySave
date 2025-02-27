using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Logging
{
    /// <summary>
    /// Implémente IBackupLogger pour logger les opérations de sauvegarde au format JSON.
    /// </summary>
    public class JsonBackupLogger : IBackupLogger
    {
        private string logDirectory;
        // Verrou statique pour synchroniser l'accès aux fichiers de log JSON
        private static readonly object _fileLock = new object();

        public JsonBackupLogger(string logDirectory)
        {
            // Le répertoire des logs peut être relatif ou absolu ; il est construit à partir du répertoire d'exécution.
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
                FileSizeUnit = "octets",
                FileTransferTime = transferTimeMs,
                FileTransferTimeUnit = "ms",
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
                FileSizeUnit = "octets",
                FileTransferTime = -1,
                FileTransferTimeUnit = "ms",
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Error = ex.Message
            };

            WriteLogEntry(entry);
        }
        
        public void LogEncryption(string filePath, int encryptionTime)
        {
            var entry = new
            {
                Operation = "Encryption",
                FilePath = filePath,
                EncryptionTime = encryptionTime,
                TimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            WriteLogEntry(entry);
        }

        /// <summary>
        /// Écrit une entrée de log dans le fichier JSON en s'assurant qu'un seul thread y accède à la fois.
        /// </summary>
        private void WriteLogEntry(object logEntry)
        {
            lock (_fileLock)
            {
                // Fichier journal quotidien nommé "yyyy-MM-dd-Log.json"
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-Log.json";
                string filePath = Path.Combine(logDirectory, fileName);

                List<object> logEntries = new List<object>();

                // 1. Si le fichier existe et n'est pas vide, tenter de lire les entrées existantes
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    try
                    {
                        string existingJson = File.ReadAllText(filePath);
                        logEntries = JsonSerializer.Deserialize<List<object>>(existingJson);
                        if (logEntries == null)
                        {
                            logEntries = new List<object>();
                        }
                    }
                    catch
                    {
                        // En cas d'erreur de désérialisation, réinitialiser la liste
                        logEntries = new List<object>();
                    }
                }

                // 2. Ajouter la nouvelle entrée à la liste
                logEntries.Add(logEntry);

                // 3. Sérialiser l'ensemble de la liste en format JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string newJson = JsonSerializer.Serialize(logEntries, options);

                // 4. Écrire (écraser) le fichier avec le JSON mis à jour
                File.WriteAllText(filePath, newJson);
            }
        }
    }
}
