using System;
using System.IO;
using System.Xml.Linq;

namespace EasySave.Logging
{
    /// <summary>
    /// Implémente IBackupLogger pour logger les opérations de sauvegarde au format XML.
    /// </summary>
    public class XmlBackupLogger : IBackupLogger
    {
        private string logDirectory;
        // Verrou statique pour synchroniser l'accès aux fichiers de logs
        private static readonly object _fileLock = new object();

        public XmlBackupLogger(string logDirectory)
        {
            // Le répertoire des logs est construit à partir du répertoire d'exécution.
            this.logDirectory = Path.Combine(AppContext.BaseDirectory, logDirectory);
            Directory.CreateDirectory(this.logDirectory);
        }

        public void LogTransfer(string backupName, string sourceFile, string destFile, long fileSize, long transferTimeMs)
        {
            XElement entry = new XElement("LogEntry",
                new XElement("Name", backupName),
                new XElement("FileSource", sourceFile),
                new XElement("FileTarget", destFile),
                new XElement("FileSize", fileSize),
                new XElement("FileSizeUnit", "octets"),
                new XElement("FileTransferTime", transferTimeMs),
                new XElement("FileTransferTimeUnit", "ms"),
                new XElement("Time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            );

            WriteLogEntry(entry);
        }

        public void LogError(string backupName, string sourceFile, string destFile, Exception ex)
        {
            XElement entry = new XElement("LogEntry",
                new XElement("Name", backupName),
                new XElement("FileSource", sourceFile),
                new XElement("FileTarget", destFile),
                new XElement("FileSize", 0),
                new XElement("FileSizeUnit", "octets"),
                new XElement("FileTransferTime", -1),
                new XElement("FileTransferTimeUnit", "ms"),
                new XElement("Time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")),
                new XElement("Error", ex.Message)
            );

            WriteLogEntry(entry);
        }
        
        public void LogEncryption(string filePath, int encryptionTime)
        {
            XElement entry = new XElement("LogEntry",
                new XElement("Operation", "Encryption"),
                new XElement("FilePath", filePath),
                new XElement("EncryptionTime", encryptionTime),
                new XElement("TimeStamp", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            );

            WriteLogEntry(entry);
        }

        /// <summary>
        /// Ecrit une entrée de log dans le fichier XML en s'assurant qu'un seul thread y accède à la fois.
        /// </summary>
        private void WriteLogEntry(XElement logEntry)
        {
            lock (_fileLock)
            {
                // Nom du fichier journal quotidien
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-Log.xml";
                string filePath = Path.Combine(logDirectory, fileName);

                XDocument doc;
                if (File.Exists(filePath))
                {
                    try
                    {
                        doc = XDocument.Load(filePath);
                    }
                    catch
                    {
                        doc = new XDocument(new XElement("LogEntries"));
                    }
                }
                else
                {
                    doc = new XDocument(new XElement("LogEntries"));
                }

                // Ajout de la nouvelle entrée
                doc.Root.Add(logEntry);
                doc.Save(filePath);
            }
        }
    }
}
