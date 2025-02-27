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
    /// Implements a DIFFERENTIAL backup strategy with parallelization,
    /// business software verification, and real-time control (pause/stop).
    ///
    /// Copy only occurs if the source file is more recent
    /// than the destination file.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            // Mark the backup as active
            backup.Status = BackupStatus.Active;

            // Check if a business software is already running before starting
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                backup.Status = BackupStatus.End;
                var ex = new Exception("Sauvegarde annulée : logiciel métier détecté avant démarrage.");
                logger.LogError(backup.Name, "", "", ex);
                Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_ErrorCopy"]
                                  .Replace("{name}", backup.Name)
                                  .Replace("{error}", "Logiciel métier détecté."));
                return;
            }

            Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Executing"].Replace("{name}", backup.Name));

            // Retrieve the file list
            var files = backup.GetFileList();
            int totalFiles = files.Count;
            if (totalFiles == 0)
            {
                backup.Status = BackupStatus.End;
                Console.WriteLine($"[DIFF] Aucun fichier à copier pour {backup.Name}. Fin immédiate.");
                return;
            }

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

            // Variables for parallel processing
            int processedFiles = 0;
            object stateLock = new object();
            long thresholdBytes = GeneralSettings.MaxLargeFileSize;
            SemaphoreSlim largeFileSemaphore = new SemaphoreSlim(1, 1);

            // Get the cancellation token for Stop
            CancellationToken token = backup.JobControl.CancellationToken;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = token
            };

            try
            {
                // Parallel processing
                Parallel.ForEach(files, parallelOptions, fileInfo =>
                {
                    // Check for cancellation
                    token.ThrowIfCancellationRequested();
                    if (backup.JobControl.IsPaused)
                    {
                        lock (stateLock)
                        {
                            state.Status = BackupStatus.Paused;
                            StateManager.UpdateState(state);
                        }
                    }
                    backup.JobControl.WaitIfPaused();

                    lock (stateLock)
                    {
                        state.Status = BackupStatus.Active;
                        StateManager.UpdateState(state);
                    }
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

                    // Once the software stops, resume the backup
                    backup.JobControl.Resume(backup);

                    // Re-check for cancellation and pause afterward
                    token.ThrowIfCancellationRequested();
                    backup.JobControl.WaitIfPaused();

                    // Check if the file should be copied (differential)
                    var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                    var destFilePath = Path.Combine(backup.TargetPath, relativePath);
                    bool needCopy = !File.Exists(destFilePath) ||
                                    (new FileInfo(destFilePath).LastWriteTime < fileInfo.LastWriteTime);

                    lock (stateLock)
                    {
                        state.SourceFilePath = fileInfo.FullName;
                        state.TargetFilePath = destFilePath;
                        StateManager.UpdateState(state);
                    }

                    if (needCopy)
                    {
                        // Create the target directory
                        Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                        // Large file handling
                        if (fileInfo.Length > thresholdBytes)
                            largeFileSemaphore.Wait();

                        try
                        {
                            var startTime = DateTime.Now;
                            FileHelper.CopyFileWithCancellation(fileInfo.FullName, destFilePath, backup);
                            long transferTimeMs = (long)((DateTime.Now - startTime).TotalMilliseconds);

                            // Log
                            logger.LogTransfer(backup.Name, fileInfo.FullName, destFilePath, fileInfo.Length, transferTimeMs);
                            Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Copied"]
                                              .Replace("{name}", fileInfo.Name));

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
                            Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_ErrorCopy"]
                                .Replace("{name}", fileInfo.Name)
                                .Replace("{error}", ex.Message));
                        }
                        finally
                        {
                            if (fileInfo.Length > thresholdBytes)
                                largeFileSemaphore.Release();
                        }
                    }
                    else
                    {
                        // File already up to date
                        Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Skipped"]
                                          .Replace("{name}", fileInfo.Name));
                    }

                    // Update progress
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

                // Normal completion (no exceptions)
                Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Finished"]
                                  .Replace("{count}", processedFiles.ToString()));
                state.Status = BackupStatus.End;
                backup.Status = BackupStatus.End;
                state.SourceFilePath = "";
                state.TargetFilePath = "";
                StateManager.UpdateState(state);
            }
            catch (OperationCanceledException)
            {
                // Stop requested
                Console.WriteLine($"[DIFF] Sauvegarde {backup.Name} arrêtée par l'utilisateur (Stop).");
                backup.Status = BackupStatus.End;
                state.Status = BackupStatus.End;
                StateManager.UpdateState(state);
                return;
            }
            catch (Exception ex)
            {
                // Other exception => Error
                backup.Status = BackupStatus.Error;
                state.Status = BackupStatus.Error;
                StateManager.UpdateState(state);

                Console.WriteLine($"[DIFF] Erreur inattendue : {ex.Message}");
                logger.LogError(backup.Name, "", "", ex);
            }
        }
    }
}
