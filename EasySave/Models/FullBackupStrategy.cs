using System;
using System.IO;

namespace EasySave.Models
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            Console.WriteLine($"[FullBackup] Executing FULL backup for '{backup.Name}'...");

            var files = backup.GetFileList();
            if (files.Count == 0)
            {
                Console.WriteLine("[FullBackup] No files to copy.");
                return;
            }

            foreach (var fileInfo in files)
            {
                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\','/');
                var destFilePath = Path.Combine(backup.TargetPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                var startTime = DateTime.Now;  // pour calculer le temps de transfert

                try
                {
                    File.Copy(fileInfo.FullName, destFilePath, true);
                    var endTime = DateTime.Now;

                    long transferTimeMs = (long)((endTime - startTime).TotalMilliseconds);

                    // On loggue le succès
                    logger.LogTransfer(
                        backup.Name,
                        fileInfo.FullName,
                        destFilePath,
                        fileInfo.Length,
                        transferTimeMs
                    );

                    Console.WriteLine($"[FullBackup] Copied: {fileInfo.Name}");
                }
                catch (Exception ex)
                {
                    // On loggue l'erreur
                    logger.LogError(backup.Name, fileInfo.FullName, destFilePath, ex);
                    Console.WriteLine($"[FullBackup] Error copying {fileInfo.Name}: {ex.Message}");
                }
            }
        }
    }
}