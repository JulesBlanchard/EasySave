using System;
using System.IO;

namespace EasySave.Models
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Sauvegarde différentielle : on ne copie que si la source est plus récente 
        /// ou si le fichier n'existe pas dans la cible.
        /// </summary>
        public void Execute(Backup backup)
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

                // Vérifier si le fichier existe déjà
                bool needCopy = false;
                if (!File.Exists(destFilePath))
                {
                    needCopy = true;
                }
                else
                {
                    // Compare LastWriteTime
                    var destInfo = new FileInfo(destFilePath);
                    if (fileInfo.LastWriteTime > destInfo.LastWriteTime)
                    {
                        needCopy = true;
                    }
                }

                if (needCopy)
                {
                    // Créer le dossier cible si nécessaire
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                    try
                    {
                        File.Copy(fileInfo.FullName, destFilePath, true);
                        copiedCount++;
                        Console.WriteLine($"[DiffBackup] Copied: {fileInfo.Name}");
                    }
                    catch (Exception ex)
                    {
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
