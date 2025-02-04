namespace EasySave.Models
{
    public class BackupState
    {
        // Nom du travail de sauvegarde
        public string Name { get; set; }
        // Chemin du fichier source actuellement en cours de copie (vide si aucun)
        public string SourceFilePath { get; set; }
        // Chemin du fichier destination actuellement en cours de copie (vide si aucun)
        public string TargetFilePath { get; set; }
        // Etat du backup : maintenant de type BackupStatus
        public BackupStatus Status { get; set; } = BackupStatus.NotStarted;
        // Nombre total de fichiers à copier
        public int TotalFilesToCopy { get; set; }
        // Taille totale de tous les fichiers à copier (en octets)
        public long TotalFilesSize { get; set; }
        // Unité de la taille totale
        public string TotalFilesSizeUnit { get; set; } = "octets";
        // Nombre de fichiers restant à copier
        public int NbFilesLeftToDo { get; set; }
        // Progression en pourcentage (0 à 100)
        public int Progression { get; set; }
        // (Optionnel) Horodatage de la dernière action
        public string LastActionTimestamp { get; set; }
    }
}