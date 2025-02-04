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
            // Log file name format: "yyyy-MM-dd-Log.json"
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-Log.json";
            string filePath = Path.Combine(logDirectory, fileName);

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                // Serialize the log entry
                string jsonLine = JsonSerializer.Serialize(logEntry, options);
                // Append the log entry to the file (each entry on a separate line)
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine(jsonLine);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[JsonBackupLogger] Error writing log: " + e.Message);
            }
        }
    }
}
