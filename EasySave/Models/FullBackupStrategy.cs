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
    /// Implémente une stratégie de sauvegarde FULL avec parallélisation, gestion des fichiers prioritaires
    /// et mise en pause si le logiciel métier est lancé.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            // Vérifier avant le démarrage si le logiciel métier est lancé
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                var ex = new Exception("Sauvegarde annulée : logiciel métier détecté avant démarrage.");
                logger.LogError(backup.Name, "", "", ex);
                Console.WriteLine("Sauvegarde annulée : logiciel métier détecté.");
                return;
            }

            Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Executing"].Replace("{name}", backup.Name));
            var allFiles = backup.GetFileList();
            int totalFiles = allFiles.Count;
            long totalSize = allFiles.Sum(f => f.Length);

            // Initialiser l'état de la sauvegarde
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

            // Récupérer la liste des extensions prioritaires
            var priorityExtensions = GeneralSettings.PriorityExtensions
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim().ToLowerInvariant()).ToList();

            // Séparer les fichiers prioritaires et non prioritaires
            var priorityFiles = allFiles.Where(file =>
                priorityExtensions.Contains(Path.GetExtension(file.FullName).ToLowerInvariant())).ToList();
            var nonPriorityFiles = allFiles.Except(priorityFiles).ToList();

            // Ajouter le nombre de fichiers prioritaires au compteur global
            PriorityManager.AddPriorityFiles(priorityFiles.Count);

            // Traiter d'abord les fichiers prioritaires en les plaçant en tête
            var sortedFiles = priorityFiles.Concat(nonPriorityFiles).ToList();

            int processedFiles = 0;
            object stateLock = new object();
            int thresholdBytes = GeneralSettings.MaxLargeFileSize;
            SemaphoreSlim largeFileSemaphore = new SemaphoreSlim(1, 1);

            Parallel.ForEach(sortedFiles, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, fileInfo =>
            {
                bool isPriority = priorityExtensions.Contains(Path.GetExtension(fileInfo.FullName).ToLowerInvariant());

                // ***** Vérification du logiciel métier avec pause notifier *****
                while (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
                {
                    // Appeler le notifier (avec gestion du flag pour éviter les popups multiples)
                    PauseNotifierEvent.RequestPause();
                    Thread.Sleep(500);
                }
                // Une fois que le logiciel métier n'est plus détecté, réinitialiser le flag.
                PauseNotifierEvent.Reset();
                // **************************************************************

                // Pour les fichiers non prioritaires, attendre tant qu'il y a encore des fichiers prioritaires en attente.
                if (!isPriority)
                {
                    while (PriorityManager.GetPendingCount() > 0)
                    {
                        Thread.Sleep(500);
                    }
                }

                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                var destFilePath = Path.Combine(backup.TargetPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                // Mise à jour de l'état (section critique)
                lock (stateLock)
                {
                    state.SourceFilePath = fileInfo.FullName;
                    state.TargetFilePath = destFilePath;
                    StateManager.UpdateState(state);
                }

                // Gestion des fichiers volumineux
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
                    Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Copied"].Replace("{name}", fileInfo.Name));

                    // Traitement du cryptage si activé
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
                finally
                {
                    if (fileInfo.Length > thresholdBytes)
                    {
                        largeFileSemaphore.Release();
                    }
                }

                // Si le fichier traité était prioritaire, décrémenter le compteur global
                if (isPriority)
                {
                    PriorityManager.DecrementPriorityFiles();
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
            Console.WriteLine(LocalizationManager.CurrentMessages["FullBackup_Finished"].Replace("{name}", backup.Name));
            state.Status = BackupStatus.End;
            state.SourceFilePath = "";
            state.TargetFilePath = "";
            StateManager.UpdateState(state);
        }
    }
}
