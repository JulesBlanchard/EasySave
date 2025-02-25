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
    /// Implémente une stratégie de sauvegarde DIFFÉRENTIELLE avec parallélisation,
    /// vérification du logiciel métier, et contrôle temps réel (pause / stop).
    ///
    /// La copie ne s'effectue que si le fichier source est plus récent
    /// que le fichier de destination.
    ///
    /// TODO : Gérer la localisation (FR/EN) pour tous les messages (console, logs, pop-ups).
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            // Marquer la sauvegarde comme active
            backup.Status = BackupStatus.Active;

            // Vérifier si un logiciel métier est déjà lancé avant de démarrer
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

            // Récupérer la liste de fichiers
            var files = backup.GetFileList();
            int totalFiles = files.Count;
            if (totalFiles == 0)
            {
                // Pas de fichier à traiter
                backup.Status = BackupStatus.End;
                Console.WriteLine($"[DIFF] Aucun fichier à copier pour {backup.Name}. Fin immédiate.");
                return;
            }

            long totalSize = files.Sum(f => f.Length);

            // Créer un BackupState pour StateManager
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

            // Variables pour le traitement parallèle
            int processedFiles = 0;
            object stateLock = new object();
            int thresholdBytes = GeneralSettings.MaxLargeFileSize;
            SemaphoreSlim largeFileSemaphore = new SemaphoreSlim(1, 1);

            // Récupérer le token d'annulation pour Stop
            CancellationToken token = backup.JobControl.CancellationToken;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = token
            };

            try
            {
                // Traitement en parallèle
                Parallel.ForEach(files, parallelOptions, fileInfo =>
                {
                    // Vérifier annulation
                    token.ThrowIfCancellationRequested();
                    // Vérifier pause
                    backup.JobControl.WaitIfPaused();

                    // Vérifier si un logiciel métier apparaît pendant la sauvegarde
                    while (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                    {
                        PauseNotifierEvent.RequestPause();
                        Thread.Sleep(500);
                    }
                    PauseNotifierEvent.Reset();

                    // Re-vérifier annulation et pause après coup
                    token.ThrowIfCancellationRequested();
                    backup.JobControl.WaitIfPaused();

                    // Vérifier si le fichier doit être copié (différentiel)
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
                        // Créer le répertoire cible
                        Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                        // Gestion des gros fichiers
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

                            // Vérifier si on doit crypter le fichier
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
                                    Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_ErrorCopy"]
                                        .Replace("{name}", fileInfo.Name)
                                        .Replace("{error}", $"Extension {fileExtension} non autorisée pour le cryptage."));
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
                        // Fichier déjà à jour
                        Console.WriteLine(LocalizationManager.CurrentMessages["DiffBackup_Skipped"]
                                          .Replace("{name}", fileInfo.Name));
                    }

                    // Mettre à jour la progression
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

                // Fin normale (aucune exception)
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
                // Stop demandé
                Console.WriteLine($"[DIFF] Sauvegarde {backup.Name} arrêtée par l'utilisateur (Stop).");
                backup.Status = BackupStatus.End;
                state.Status = BackupStatus.End;
                StateManager.UpdateState(state);
                return;
            }
            catch (Exception ex)
            {
                // Autre exception => Error
                backup.Status = BackupStatus.Error;
                state.Status = BackupStatus.Error;
                StateManager.UpdateState(state);

                Console.WriteLine($"[DIFF] Erreur inattendue : {ex.Message}");
                logger.LogError(backup.Name, "", "", ex);
            }
        }
    }
}
