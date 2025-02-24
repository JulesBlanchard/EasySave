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
    /// Implémente une stratégie de sauvegarde FULL avec parallélisation, gestion des fichiers prioritaires,
    /// vérification du logiciel métier et contrôle temps réel (pause/play/stop).
    /// /// TODO : Gérer la localisation (FR/EN) pour tous les messages 
    ///        (console, logs, pop-ups). 
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            // 1) Indiquer que la sauvegarde démarre
            backup.Status = BackupStatus.Active;
            Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Executing"].Replace("{name}", backup.Name));

            // Vérifier si un logiciel métier est déjà lancé
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

            // 2) Récupérer la liste de tous les fichiers
            var allFiles = backup.GetFileList();
            int totalFiles = allFiles.Count;
            if (totalFiles == 0)
            {
                // S'il n'y a aucun fichier, on termine immédiatement
                backup.Status = BackupStatus.End;
                Console.WriteLine($"[FULL] Aucune ressource à copier pour {backup.Name}. Fin immédiate.");
                return;
            }

            long totalSize = allFiles.Sum(f => f.Length);

            // 3) Créer l'objet BackupState pour le suivi (StateManager)
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

            // 4) Gestion des fichiers prioritaires
            var priorityExtensions = GeneralSettings.PriorityExtensions
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim().ToLowerInvariant()).ToList();
            var priorityFiles = allFiles.Where(file =>
                priorityExtensions.Contains(Path.GetExtension(file.FullName).ToLowerInvariant())).ToList();
            var nonPriorityFiles = allFiles.Except(priorityFiles).ToList();

            // Ajouter le nombre de fichiers prioritaires au compteur global
            PriorityManager.AddPriorityFiles(priorityFiles.Count);

            // Traiter d'abord les prioritaires
            var sortedFiles = priorityFiles.Concat(nonPriorityFiles).ToList();

            // 5) Variables pour le traitement parallèle
            int processedFiles = 0;
            object stateLock = new object();
            int thresholdBytes = GeneralSettings.MaxLargeFileSize;
            SemaphoreSlim largeFileSemaphore = new SemaphoreSlim(1, 1);

            // Récupération du token d'annulation pour le Stop
            CancellationToken token = backup.JobControl.CancellationToken;

            // 6) Préparer les options Parallel
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = token
            };

            try
            {
                Parallel.ForEach(sortedFiles, parallelOptions, fileInfo =>
                {
                    // Vérifier si Stop a été demandé
                    token.ThrowIfCancellationRequested();

                    // Vérifier si Pause a été enclenchée
                    backup.JobControl.WaitIfPaused();

                    bool isPriority = priorityExtensions.Contains(Path.GetExtension(fileInfo.FullName).ToLowerInvariant());

                    // TODO : Pendant la détection du logiciel métier, 
                    //        on voudrait griser le bouton Pause dans l'IHM de suivi.
                    //        Actuellement, on met en pause la sauvegarde automatiquement 
                    //        via PauseNotifierEvent.RequestPause().
                    // Vérifier si un logiciel métier apparaît en cours d'exécution
                    while (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                    {
                        PauseNotifierEvent.RequestPause();
                        Thread.Sleep(500);
                    }
                    PauseNotifierEvent.Reset();

                    // Re-vérifier annulation et pause
                    token.ThrowIfCancellationRequested();
                    backup.JobControl.WaitIfPaused();

                    // Attendre si c'est un fichier non prioritaire mais qu'il reste des priorités
                    if (!isPriority)
                    {
                        while (PriorityManager.GetPendingCount() > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            backup.JobControl.WaitIfPaused();
                            Thread.Sleep(500);
                        }
                    }

                    // Création du dossier cible
                    var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                    var destFilePath = Path.Combine(backup.TargetPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                    // Mise à jour de l'état avant la copie
                    lock (stateLock)
                    {
                        state.SourceFilePath = fileInfo.FullName;
                        state.TargetFilePath = destFilePath;
                        StateManager.UpdateState(state);
                    }

                    // Fichiers volumineux : utiliser un sémaphore pour bloquer le transfert concurrent
                    if (fileInfo.Length > thresholdBytes)
                    {
                        largeFileSemaphore.Wait();
                    }

                    try
                    {
                        // Copie du fichier
                        var startTime = DateTime.Now;
                        FileHelper.CopyFileWithCancellation(fileInfo.FullName, destFilePath, backup);
                        long transferTimeMs = (long)((DateTime.Now - startTime).TotalMilliseconds);

                        // Logger la copie
                        logger.LogTransfer(backup.Name, fileInfo.FullName, destFilePath, fileInfo.Length, transferTimeMs);
                        Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Copied"].Replace("{name}", fileInfo.Name));

                        // Cryptage si nécessaire
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
                                Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_ErrorCopy"]
                                    .Replace("{name}", fileInfo.Name)
                                    .Replace("{error}", $"Extension {fileExtension} non autorisée pour le cryptage."));
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
                        // Libérer le sémaphore si fichier volumineux
                        if (fileInfo.Length > thresholdBytes)
                        {
                            largeFileSemaphore.Release();
                        }
                    }

                    // Si le fichier était prioritaire, décrémenter le compteur
                    if (isPriority)
                    {
                        PriorityManager.DecrementPriorityFiles();
                    }

                    // Mise à jour de la progression et de l'état
                    lock (stateLock)
                    {
                        processedFiles++;
                        // Empêcher le pourcentage de dépasser 100
                        int progress = (int)((processedFiles / (double)totalFiles) * 100);
                        progress = Math.Min(progress, 100);
                        state.NbFilesLeftToDo = totalFiles - processedFiles;
                        state.Progression = progress;
                        backup.Progression = progress; // mise à jour de backup.Progression

                        StateManager.UpdateState(state);
                    }
                });

                // Si on arrive ici sans exception => terminé normalement
                Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Finished"].Replace("{name}", backup.Name));
                state.Status = BackupStatus.End;
                backup.Status = BackupStatus.End;
                state.SourceFilePath = "";
                state.TargetFilePath = "";
                StateManager.UpdateState(state);
            }
            catch (OperationCanceledException)
            {
                // Stop demandé => on sort proprement
                Console.WriteLine($"[FULL] Sauvegarde {backup.Name} arrêtée par l'utilisateur (Stop).");
                backup.Status = BackupStatus.End;
                state.Status = BackupStatus.End;
                StateManager.UpdateState(state);
                return;

            }
            catch (Exception ex)
            {
                // Autre exception => mettre en Error
                backup.Status = BackupStatus.Error;
                state.Status = BackupStatus.Error;
                StateManager.UpdateState(state);

                Console.WriteLine($"[FULL] Erreur inattendue : {ex.Message}");
                logger.LogError(backup.Name, "", "", ex);
            }
        }
    }
}
