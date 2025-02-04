using System;
using System.IO;

namespace EasySave.Models
{
    public class FullBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Sauvegarde complète : copie tous les fichiers du répertoire source 
        /// vers le répertoire cible, en écrasant si besoin.
        /// </summary>
        public void Execute(Backup backup)
        {
            Console.WriteLine($"[FullBackup] Executing FULL backup for '{backup.Name}'...");
            
            // On récupère la liste de fichiers depuis backup.GetFileList().
            var files = backup.GetFileList();
            if (files.Count == 0)
            {
                Console.WriteLine("[FullBackup] No files to copy.");
                return;
            }

            foreach (var fileInfo in files)
            {
                // Chemin relatif par rapport au source
                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length).TrimStart('\\','/');
                // Construct le chemin cible
                var destFilePath = Path.Combine(backup.TargetPath, relativePath);

                // Créer le dossier cible s'il n'existe pas
                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                try
                {
                    File.Copy(fileInfo.FullName, destFilePath, true);
                    Console.WriteLine($"[FullBackup] Copied: {fileInfo.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FullBackup] Error copying file {fileInfo.Name}: {ex.Message}");
                }
            }

            Console.WriteLine($"[FullBackup] Finished copying {files.Count} file(s).");
        }
    }
}