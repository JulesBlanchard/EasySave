using System;
using System.IO;

namespace EasySave.Models
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup, IBackupLogger logger)
        {
            Console.WriteLine($"[DiffBackup] Executing DIFFERENTIAL backup for '{backup.Name}'...");

            var files = backup.GetFileList();
            if (files.Count == 0)
            {
                Console.WriteLine("[DiffBackup] No files to copy.");
                return;
            }

            int copiedCount = 0;
            foreach (var fileInfo in files)
            {
                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\','/');
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

                        // Log succès
                        logger.LogTransfer(
                            backup.Name,
                            fileInfo.FullName,
                            destFilePath,
                            fileInfo.Length,
                            transferTimeMs
                        );

                        Console.WriteLine($"[DiffBackup] Copied: {fileInfo.Name}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(backup.Name, fileInfo.FullName, destFilePath, ex);
                        Console.WriteLine($"[DiffBackup] Error copying file {fileInfo.Name}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"[DiffBackup] Skipped (up-to-date): {fileInfo.Name}");
                }
            }

            Console.WriteLine($"[DiffBackup] Finished. Copied {copiedCount} file(s).");
        }
    }
}
