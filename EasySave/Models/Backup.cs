using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.IO;

namespace EasySave.Models
{
    public class Backup
    {
        // Propriétés avec getters et setters publics pour la désérialisation
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        // Propriété pour stocker le type de sauvegarde ("full" ou "diff")
        public string BackupType { get; set; }

        // Cette propriété ne sera pas sérialisée/désérialisée car on la recréera ensuite
        [JsonIgnore]
        public IBackupStrategy Strategy { get; set; }

        // Constructeur par défaut nécessaire pour la désérialisation
        public Backup() { }

        // Constructeur habituel pour la création en code
        public Backup(string name, string source, string target, string backupType = null)
        {
            Name = name;
            SourcePath = source;
            TargetPath = target;
            BackupType = backupType;
        }

        /// <summary>
        /// Retourne la liste des fichiers du répertoire source (et sous-répertoires)
        /// </summary>
        public List<FileInfo> GetFileList()
        {
            var result = new List<FileInfo>();
            if (Directory.Exists(SourcePath))
            {
                var allFiles = Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories);
                foreach (var f in allFiles)
                {
                    result.Add(new FileInfo(f));
                }
            }
            return result;
        }

        /// <summary>
        /// Exécute le backup en passant le logger à la stratégie associée.
        /// </summary>
        public void Execute(IBackupLogger logger)
        {
            if (Strategy != null)
            {
                Strategy.Execute(this, logger);
            }
            else
            {
                System.Console.WriteLine($"[Backup] Aucune stratégie définie pour '{Name}'. Rien à faire.");
            }
        }
    }
}
