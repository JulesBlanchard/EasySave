using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Logging
{
    /// <summary>
    /// Implements IBackupLogger to log backup operations in JSON format.
    /// </summary>
    public class JsonBackupLogger : IBackupLogger
    {
        private string logDirectory;

        public JsonBackupLogger(string logDirectory)
        {
            // The log directory can be relative or absolute; it's built from the execution directory.
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
                FileSizeUnit = "octets",            // Unit for file size
                FileTransferTime = transferTimeMs,
                FileTransferTimeUnit = "ms",         // Unit for transfer time
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
                FileSizeUnit = "octets",            // Unit for file size (even on error)
                FileTransferTime = -1,              // Negative value indicates error
                FileTransferTimeUnit = "ms",
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Error = ex.Message
            };

            WriteLogEntry(entry);
        }

        private void WriteLogEntry(object logEntry)
        {
            // Daily log file named "yyyy-MM-dd-Log.json"
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-Log.json";
            string filePath = Path.Combine(logDirectory, fileName);

            List<object> logEntries = new List<object>();

            // 1. If the file exists and is not empty, attempt to read existing entries
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                try
                {
                    string existingJson = File.ReadAllText(filePath);
                    // Deserialize the existing JSON array into a list
                    logEntries = JsonSerializer.Deserialize<List<object>>(existingJson);
                    if (logEntries == null)
                    {
                        logEntries = new List<object>();
                    }
                }
                catch
                {
                    // If the file is not valid JSON, re-initialize the list
                    logEntries = new List<object>();
                }
            }

            // 2. Add the new entry to the list
            logEntries.Add(logEntry);

            // 3. Serialize the entire list as a JSON array
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string newJson = JsonSerializer.Serialize(logEntries, options);

            // 4. Overwrite the file with the updated JSON array
            File.WriteAllText(filePath, newJson);
        }
    }
}
