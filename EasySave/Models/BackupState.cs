using EasySave.Models;

public class BackupState
{
    // Name of the backup.
    public string Name { get; set; }
    // Current source path.
    public string SourceFilePath { get; set; }
    // Current target path.
    public string TargetFilePath { get; set; }
    // Backup status.
    public BackupStatus Status { get; set; } = BackupStatus.NotStarted;
    // Total number of files to copy.
    public int TotalFilesToCopy { get; set; }
    // Total file size (in bytes).
    public long TotalFilesSize { get; set; }
    // Size unit.
    public string TotalFilesSizeUnit { get; set; } = "bytes";
    // Number of remaining files.
    public int NbFilesLeftToDo { get; set; }
    // Progress in percentage.
    public int Progression { get; set; }
    // Timestamp of the last action.
    public string LastActionTimestamp { get; set; }
    // Property for selection in the remote console.
    public bool IsSelected { get; set; }

}