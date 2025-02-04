using System;
using System.IO;
using System.Linq;
using EasySave.Logging;
using EasySave.Utils;

namespace EasySave.Models
{
    /// <summary>
    /// Implements a full backup strategy. Copies all files from the source to the target.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
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
