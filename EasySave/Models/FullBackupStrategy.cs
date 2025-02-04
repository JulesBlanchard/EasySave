using System;
using System.IO;
using System.Linq;
using EasySave.Logging;

namespace EasySave.Models
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            Console.WriteLine($"[FullBackup] Exécution d'une sauvegarde FULL pour '{backup.Name}'...");

            var files = backup.GetFileList();
            int totalFiles = files.Count;
            long totalSize = files.Sum(f => f.Length);

            // Initialisation de l'état avec le format demandé
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
                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\', '/');
                var destFilePath = Path.Combine(backup.TargetPath, relativePath);

                // Création du répertoire cible si nécessaire
                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                // Mise à jour de l'état avec les fichiers en cours de traitement
                state.SourceFilePath = fileInfo.FullName;
                state.TargetFilePath = destFilePath;
                StateManager.UpdateState(state);

                var startTime = DateTime.Now;
                try
                {
                    File.Copy(fileInfo.FullName, destFilePath, true);
                    var endTime = DateTime.Now;
                    long transferTimeMs = (long)((endTime - startTime).TotalMilliseconds);

                    // Loggue le transfert au format imposé
                    logger.LogTransfer(backup.Name, fileInfo.FullName, destFilePath, fileInfo.Length, transferTimeMs);
                    Console.WriteLine($"[FullBackup] Copié : {fileInfo.Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError(backup.Name, fileInfo.FullName, destFilePath, ex);
                    Console.WriteLine($"[FullBackup] Erreur lors de la copie de {fileInfo.Name} : {ex.Message}");
                }

                processedFiles++;
                state.NbFilesLeftToDo = totalFiles - processedFiles;
                state.Progression = (int)((processedFiles / (double)totalFiles) * 100);
                StateManager.UpdateState(state);

                // Affichage de la barre de progression et du temps restant estimé
                TimeSpan elapsed = DateTime.Now - startTimeGlobal;
                double averageTimePerFile = elapsed.TotalMilliseconds / processedFiles;
                int filesRemaining = totalFiles - processedFiles;
                double estimatedMsRemaining = filesRemaining * averageTimePerFile;
                Console.Write($"\rProgression : {state.Progression}% - Temps restant estimé : {TimeSpan.FromMilliseconds(estimatedMsRemaining):hh\\:mm\\:ss}");
            }
            Console.WriteLine(); // Passage à la ligne après la boucle

            // Fin du backup : on passe l'état à "END" et on vide les chemins en cours
            state.Status = BackupStatus.End;
            state.SourceFilePath = "";
            state.TargetFilePath = "";
            StateManager.UpdateState(state);

            Console.WriteLine($"[FullBackup] Sauvegarde terminée pour '{backup.Name}'.");
        }
    }
}
