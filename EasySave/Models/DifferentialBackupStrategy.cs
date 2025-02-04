using System;
using System.IO;
using System.Linq;

namespace EasySave.Models
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            Console.WriteLine($"[DiffBackup] Exécution d'une sauvegarde DIFFÉRENTIELLE pour '{backup.Name}'...");

            var files = backup.GetFileList();
            int totalFiles = files.Count;
            long totalSize = files.Sum(f => f.Length);

            // Initialisation de l'état avec le format imposé
            var state = new BackupState
            {
                Name = backup.Name,
                SourceFilePath = "",
                TargetFilePath = "",
                State = "ACTIVE",
                TotalFilesToCopy = totalFiles,
                TotalFilesSize = totalSize,
                NbFilesLeftToDo = totalFiles,
                Progression = 0
            };
            StateManager.UpdateState(state);

            DateTime startTimeGlobal = DateTime.Now;
            int processedFiles = 0;
            int copiedCount = 0;

            foreach (var fileInfo in files)
            {
                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                var destFilePath = Path.Combine(backup.TargetPath, relativePath);

                bool needCopy = false;
                if (!File.Exists(destFilePath))
                {
                    needCopy = true;
                }
                else
                {
                    var destInfo = new FileInfo(destFilePath);
                    if (fileInfo.LastWriteTime > destInfo.LastWriteTime)
                    {
                        needCopy = true;
                    }
                }

                // Mise à jour de l'état : fichier en cours de traitement
                state.SourceFilePath = fileInfo.FullName;
                state.TargetFilePath = destFilePath;
                StateManager.UpdateState(state);

                var startTime = DateTime.Now;
                if (needCopy)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                    try
                    {
                        File.Copy(fileInfo.FullName, destFilePath, true);
                        copiedCount++;
                        var endTime = DateTime.Now;
                        long transferTimeMs = (long)((endTime - startTime).TotalMilliseconds);

                        // Loggue le transfert au format imposé
                        logger.LogTransfer(backup.Name, fileInfo.FullName, destFilePath, fileInfo.Length, transferTimeMs);
                        Console.WriteLine($"[DiffBackup] Copié : {fileInfo.Name}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(backup.Name, fileInfo.FullName, destFilePath, ex);
                        Console.WriteLine($"[DiffBackup] Erreur lors de la copie de {fileInfo.Name} : {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"[DiffBackup] Ignoré (à jour) : {fileInfo.Name}");
                }

                processedFiles++;
                state.NbFilesLeftToDo = totalFiles - processedFiles;
                state.Progression = (int)((processedFiles / (double)totalFiles) * 100);
                StateManager.UpdateState(state);

                // Affichage de la progression et du temps restant estimé
                TimeSpan elapsed = DateTime.Now - startTimeGlobal;
                double averageTimePerFile = elapsed.TotalMilliseconds / processedFiles;
                int filesRemaining = totalFiles - processedFiles;
                double estimatedMsRemaining = filesRemaining * averageTimePerFile;
                Console.Write($"\rProgression : {state.Progression}% - Temps restant estimé : {TimeSpan.FromMilliseconds(estimatedMsRemaining):hh\\:mm\\:ss}");
            }
            Console.WriteLine(); // Passage à la ligne après la boucle

            Console.WriteLine($"[DiffBackup] Terminé. {copiedCount} fichier(s) copié(s).");

            // Fin du backup : on passe l'état à "END" et on vide les chemins en cours
            state.State = "END";
            state.SourceFilePath = "";
            state.TargetFilePath = "";
            StateManager.UpdateState(state);
        }
    }
}
