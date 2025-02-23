using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Logging;
using EasySave.Utils;

namespace EasySave.Models
{
    /// <summary>
    /// Implémente une stratégie de sauvegarde DIFFÉRENTIELLE avec parallélisation.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            // Vérifier si le logiciel métier est lancé avant le démarrage
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                var ex = new Exception("Sauvegarde annulée : logiciel métier détecté avant démarrage.");
                logger.LogError(backup.Name, "", "", ex);
                Console.WriteLine("Sauvegarde annulée : logiciel métier détecté.");
                return;
            }

            Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Executing"].Replace("{name}", backup.Name));

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
            int copiedCount = 0;
            int processedFiles = 0;
            object stateLock = new object();

            // Seuil pour les fichiers volumineux (défini dans GeneralSettings)
            int thresholdBytes = GeneralSettings.MaxLargeFileSize;
            // Semaphore pour limiter à 1 transfert simultané pour un fichier volumineux
            SemaphoreSlim largeFileSemaphore = new SemaphoreSlim(1, 1);

            // Parcours des fichiers en parallèle
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, fileInfo =>
            {
                // Mettre en pause si un logiciel métier est lancé
                while (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                {
                    Thread.Sleep(500);
                }

                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                var destFilePath = Path.Combine(backup.TargetPath, relativePath);
                bool needCopy = !File.Exists(destFilePath) ||
                                (new FileInfo(destFilePath).LastWriteTime < fileInfo.LastWriteTime);

                // Mise à jour de l’état (section critique)
                lock (stateLock)
                {
                    state.SourceFilePath = fileInfo.FullName;
                    state.TargetFilePath = destFilePath;
                    StateManager.UpdateState(state);
                }

                if (needCopy)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                    
                    // Si le fichier est volumineux, attendre le sémaphore
                    if (fileInfo.Length > thresholdBytes)
                    {
                        largeFileSemaphore.Wait();
                    }

                    try
                    {
                        var startTime = DateTime.Now;
                        File.Copy(fileInfo.FullName, destFilePath, true);
                        long transferTimeMs = (long)((DateTime.Now - startTime).TotalMilliseconds);
                        logger.LogTransfer(backup.Name, fileInfo.FullName, destFilePath, fileInfo.Length, transferTimeMs);
                        Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Copied"].Replace("{name}", fileInfo.Name));

                        // Traitement du cryptage si activé
                        if (backup.ShouldEncrypt)
                        {
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

                        lock (stateLock)
                        {
                            copiedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(backup.Name, fileInfo.FullName, destFilePath, ex);
                        Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_ErrorCopy"]
                            .Replace("{name}", fileInfo.Name)
                            .Replace("{error}", ex.Message));
                    }
                    finally
                    {
                        if (fileInfo.Length > thresholdBytes)
                        {
                            largeFileSemaphore.Release();
                        }
                    }
                }
                else
                {
                    Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Skipped"].Replace("{name}", fileInfo.Name));
                }

                // Mise à jour de la progression
                lock (stateLock)
                {
                    processedFiles++;
                    state.NbFilesLeftToDo = totalFiles - processedFiles;
                    state.Progression = (int)((processedFiles / (double)totalFiles) * 100);
                    StateManager.UpdateState(state);
                }
            });

            Console.WriteLine();
            Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Finished"].Replace("{count}", copiedCount.ToString()));

            state.Status = BackupStatus.End;
            state.SourceFilePath = "";
            state.TargetFilePath = "";
            StateManager.UpdateState(state);
        }
    }
}
