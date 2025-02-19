using System;
using System.IO;
using System.Linq;
using EasySave.Logging;
using EasySave.Utils;

namespace EasySave.Models
{
    /// <summary>
    /// Implémente une stratégie de sauvegarde FULL.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            // Si le logiciel métier est déjà lancé, on ne démarre pas la sauvegarde.
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                var ex = new Exception("Sauvegarde annulée : logiciel métier détecté avant démarrage.");
                logger.LogError(backup.Name, "", "", ex);
                Console.WriteLine("Sauvegarde annulée : logiciel métier détecté.");
                return;
            }

            Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Executing"].Replace("{name}", backup.Name));

            var files = backup.GetFileList();
            int totalFiles = files.Count;
            long totalSize = files.Sum(f => f.Length);

            var state = new BackupState
            {
                Name = backup.Name,
                SourceFilePath = "",
                TargetFilePath = "",
                Status = BackupStatus.Active,
                TotalFilesToCopy = totalFiles,
                TotalFilesSize = totalSize,
                NbFilesLeftToDo = totalFiles,
                Progression = 0
            };
            StateManager.UpdateState(state);

            DateTime startTimeGlobal = DateTime.Now;
            int processedFiles = 0;

            foreach (var fileInfo in files)
            {
                // Avant de lancer la copie d'un nouveau fichier, on vérifie si le logiciel métier est apparu.
                if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                {
                    var ex = new Exception("Sauvegarde interrompue : logiciel métier détecté durant l'exécution.");
                    logger.LogError(backup.Name, fileInfo.FullName, "", ex);
                    Console.WriteLine("Sauvegarde interrompue : logiciel métier détecté. Fin de la sauvegarde en cours.");
                    break; // On arrête le traitement des fichiers suivants.
                }

                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                var destFilePath = Path.Combine(backup.TargetPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                state.SourceFilePath = fileInfo.FullName;
                state.TargetFilePath = destFilePath;
                StateManager.UpdateState(state);

                var startTime = DateTime.Now;
                try
                {
                    File.Copy(fileInfo.FullName, destFilePath, true);
                    long transferTimeMs = (long)((DateTime.Now - startTime).TotalMilliseconds);
                    logger.LogTransfer(backup.Name, fileInfo.FullName, destFilePath, fileInfo.Length, transferTimeMs);
                    Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Copied"].Replace("{name}", fileInfo.Name));
                    
                    // --- CRYPTAGE si activé ---
                    if (backup.ShouldEncrypt)
                    {
                        // Récupérer l'extension du fichier et la comparer aux extensions autorisées
                        string fileExtension = Path.GetExtension(fileInfo.FullName).ToLowerInvariant();
                        var allowedExtensions = GeneralSettings.AllowedEncryptionFileTypes
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(ext => ext.Trim().ToLowerInvariant());
                        if (allowedExtensions.Contains(fileExtension))
                        {
                            var fileManager = new CryptoSoft.FileManager(destFilePath, backup.EncryptionKey);
                            int encryptionTime = fileManager.TransformFile();
                            logger.LogEncryption(destFilePath, encryptionTime);
                            Console.WriteLine($"Fichier crypté en {encryptionTime} ms : {fileInfo.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"Le fichier {fileInfo.Name} (extension {fileExtension}) n'est pas autorisé pour le cryptage.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(backup.Name, fileInfo.FullName, destFilePath, ex);
                    Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_ErrorCopy"]
                        .Replace("{name}", fileInfo.Name)
                        .Replace("{error}", ex.Message));
                }

                processedFiles++;
                state.NbFilesLeftToDo = totalFiles - processedFiles;
                state.Progression = (int)((processedFiles / (double)totalFiles) * 100);
                StateManager.UpdateState(state);

                TimeSpan elapsed = DateTime.Now - startTimeGlobal;
                double averageTimePerFile = elapsed.TotalMilliseconds / processedFiles;
                int filesRemaining = totalFiles - processedFiles;
                double estimatedMsRemaining = filesRemaining * averageTimePerFile;
                string progressMsg = LocalizationManager.CurrentMessages["FullBackup_Progress"]
                                        .Replace("{progress}", state.Progression.ToString())
                                        .Replace("{time}", TimeSpan.FromMilliseconds(estimatedMsRemaining).ToString(@"hh\:mm\:ss"));
                Console.Write($"\r{progressMsg}");
            }
            Console.WriteLine();
            Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Finished"].Replace("{name}", backup.Name));

            state.Status = BackupStatus.End;
            state.SourceFilePath = "";
            state.TargetFilePath = "";
            StateManager.UpdateState(state);
        }
    }
}
