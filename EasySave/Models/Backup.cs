using System;
using System.Collections.Generic;
using System.IO;

namespace EasySave.Models
{
    public class Backup
    {
        public string Name { get; private set; }
        public string SourcePath { get; private set; }
        public string TargetPath { get; private set; }

        // Référence à la stratégie de sauvegarde (optionnelle pour le moment).
        public IBackupStrategy Strategy { get; set; }

        public Backup(string name, string source, string target)
        {
            Name = name;
            SourcePath = source;
            TargetPath = target;
        }

        /// <summary>
        /// Exécute la sauvegarde en utilisant la stratégie.
        /// </summary>
        public void Execute()
        {
            // S'il y a une stratégie, on l'appelle ; sinon, on affiche un message minimal.
            if (Strategy != null)
            {
                Strategy.Execute(this);
            }
            else
            {
                Console.WriteLine($"[Backup] No strategy set for '{Name}'. Nothing to do.");
            }
        }

        /// <summary>
        /// Méthode éventuelle pour lister tous les fichiers du répertoire source.
        /// (Utile dans une future feature de copie).
        /// </summary>
        /// <returns>Une liste de FileInfo.</returns>
        public List<FileInfo> GetFileList()
        {
            var files = new List<FileInfo>();
            if (Directory.Exists(SourcePath))
            {
                string[] allFiles = Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories);
                foreach (var file in allFiles)
                {
                    files.Add(new FileInfo(file));
                }
            }
            return files;
        }
    }
}