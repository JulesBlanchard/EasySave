using EasySave.Models;

public class BackupState
{
    // Nom de la sauvegarde.
    public string Name { get; set; }
    // Chemin source en cours.
    public string SourceFilePath { get; set; }
    // Chemin cible en cours.
    public string TargetFilePath { get; set; }
    // Statut de la sauvegarde.
    public BackupStatus Status { get; set; } = BackupStatus.NotStarted;
    // Nombre total de fichiers à copier.
    public int TotalFilesToCopy { get; set; }
    // Taille totale des fichiers (en octets).
    public long TotalFilesSize { get; set; }
    // Unité de la taille.
    public string TotalFilesSizeUnit { get; set; } = "octets";
    // Nombre de fichiers restant.
    public int NbFilesLeftToDo { get; set; }
    // Progression en pourcentage.
    public int Progression { get; set; }
    // Timestamp de la dernière action.
    public string LastActionTimestamp { get; set; }
    
    // Propriété pour la sélection dans la console distante.
    public bool IsSelected { get; set; }
}