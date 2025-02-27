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
    /// Implements a FULL backup strategy with parallelization, priority file management,
    /// business software verification, and real-time control (pause/play/stop).
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            backup.Status = BackupStatus.Active;
            Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Executing"].Replace("{name}", backup.Name));

            // Check if a business software is already running
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                backup.Status = BackupStatus.End;
                var ex = new Exception("Sauvegarde annulée : logiciel métier détecté avant démarrage.");
                logger.LogError(backup.Name, "", "", ex);
                Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_ErrorCopy"]
                    .Replace("{name}", backup.Name)
                    .Replace("{error}", "Logiciel métier détecté."));
                return;
            }

            // Retrieve the list of all files
            var allFiles = backup.GetFileList();
            int totalFiles = allFiles.Count;
            if (totalFiles == 0)
            {
                // If there are no files, terminate immediately
                backup.Status = BackupStatus.End;
                Console.WriteLine($"[FULL] Aucune ressource à copier pour {backup.Name}. Fin immédiate.");
                return;
            }

            long totalSize = allFiles.Sum(f => f.Length);

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

            // Manage priority files
            var priorityExtensions = GeneralSettings.PriorityExtensions
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim().ToLowerInvariant()).ToList();
            var priorityFiles = allFiles.Where(file =>
                priorityExtensions.Contains(Path.GetExtension(file.FullName).ToLowerInvariant())).ToList();
            var nonPriorityFiles = allFiles.Except(priorityFiles).ToList();

            // Add the number of priority files to the global counter
            PriorityManager.AddPriorityFiles(priorityFiles.Count);

            // Process priority files first
            var sortedFiles = priorityFiles.Concat(nonPriorityFiles).ToList();
            
            // Variables for parallel processing
            int processedFiles = 0;
            object stateLock = new object();
            long thresholdBytes = GeneralSettings.MaxLargeFileSize;
            SemaphoreSlim largeFileSemaphore = new SemaphoreSlim(1, 1);

            // Retrieve the cancellation token for Stop
            CancellationToken token = backup.JobControl.CancellationToken;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = token
            };

            try
            {
                Parallel.ForEach(sortedFiles, parallelOptions, fileInfo =>
                {
                    // Check if Stop was requested
                    token.ThrowIfCancellationRequested();

                    if (backup.JobControl.IsPaused)
                    {
                        lock (stateLock)
                        {
                            state.Status = BackupStatus.Paused;
                            StateManager.UpdateState(state);
                        }
                    }
                    // Check if Pause was triggered
                    backup.JobControl.WaitIfPaused();
                    lock (stateLock)
                    {
                        state.Status = BackupStatus.Active;
                        StateManager.UpdateState(state);
                    }
                    bool isPriority = priorityExtensions.Contains(Path.GetExtension(fileInfo.FullName).ToLowerInvariant());
                    
                    // If the business software is detected, pause the backup
                    if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                    {
                        backup.JobControl.Pause(backup);
                    }

                    // Wait until the business software stops running
                    while (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                    {
                        Thread.Sleep(500);
                    }

                    backup.JobControl.Resume(backup);
                    // Re-check for cancellation and pause
                    token.ThrowIfCancellationRequested();
                    backup.JobControl.WaitIfPaused();

                    // Wait if it is a non-priority file but there are still priority files
                    if (!isPriority)
                    {
                        while (PriorityManager.GetPendingCount() > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            backup.JobControl.WaitIfPaused();
                            Thread.Sleep(500);
                        }
                    }

                    // Create the target directory
                    var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                    var destFilePath = Path.Combine(backup.TargetPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                    // Update state before copying
                    lock (stateLock)
                    {
                        state.SourceFilePath = fileInfo.FullName;
                        state.TargetFilePath = destFilePath;
                        StateManager.UpdateState(state);
                    }

                    // Large file handling: use a semaphore to block concurrent transfers
                    if (fileInfo.Length > thresholdBytes)
                    {
                        largeFileSemaphore.Wait();
                    }

                    try
                    {
                        // File copy
                        var startTime = DateTime.Now;
                        FileHelper.CopyFileWithCancellation(fileInfo.FullName, destFilePath, backup);
                        long transferTimeMs = (long)((DateTime.Now - startTime).TotalMilliseconds);

                        // Log copy
                        logger.LogTransfer(backup.Name, fileInfo.FullName, destFilePath, fileInfo.Length, transferTimeMs);
                        Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Copied"].Replace("{name}", fileInfo.Name));

                        // Encryption if necessary
                        if (backup.ShouldEncrypt)
                        {
                            string fileExtension = Path.GetExtension(fileInfo.FullName).ToLowerInvariant();
                            var allowedExtensions = GeneralSettings.AllowedEncryptionFileTypes
                                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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
                                Console.WriteLine($"Fichier copié sans cryptage (extension non autorisée) : {fileInfo.Name}");
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
                    finally
                    {
                        // Release the semaphore if the file was large
                        if (fileInfo.Length > thresholdBytes)
                        {
                            largeFileSemaphore.Release();
                        }
                    }

                    // If the file was priority, decrement the counter
                    if (isPriority)
                    {
                        PriorityManager.DecrementPriorityFiles();
                    }

                    // Update progress and state
                    lock (stateLock)
                    {
                        processedFiles++;
                        int progress = (int)((processedFiles / (double)totalFiles) * 100);
                        progress = Math.Min(progress, 100);
                        state.NbFilesLeftToDo = totalFiles - processedFiles;
                        state.Progression = progress;
                        backup.Progression = progress; 

                        StateManager.UpdateState(state);
                    }
                });

                Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Finished"].Replace("{name}", backup.Name));
                state.Status = BackupStatus.End;
                backup.Status = BackupStatus.End;
                state.SourceFilePath = "";
                state.TargetFilePath = "";
                StateManager.UpdateState(state);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[FULL] Sauvegarde {backup.Name} arrêtée par l'utilisateur (Stop).");
                backup.Status = BackupStatus.End;
                state.Status = BackupStatus.End;
                StateManager.UpdateState(state);
                return;

            }
            catch (Exception ex)
            {
                backup.Status = BackupStatus.Error;
                state.Status = BackupStatus.Error;
                StateManager.UpdateState(state);

                Console.WriteLine($"[FULL] Erreur inattendue : {ex.Message}");
                logger.LogError(backup.Name, "", "", ex);
            }
        }
    }
}
