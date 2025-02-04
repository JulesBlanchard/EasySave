namespace EasySave.Models
{
    /// <summary>
    /// Represents the state of a backup, including progress and file statistics.
    /// </summary>
    public class BackupState
    {
        // Backup name.
        public string Name { get; set; }
        // Current source file path being processed (empty if none).
        public string SourceFilePath { get; set; }
        // Current target file path being processed (empty if none).
        public string TargetFilePath { get; set; }
        // Backup status (enum: NotStarted, Active, End, Error).
        public BackupStatus Status { get; set; } = BackupStatus.NotStarted;
        // Total number of files to copy.
        public int TotalFilesToCopy { get; set; }
        // Total size of all files to copy (in bytes).
        public long TotalFilesSize { get; set; }
        // Unit for the total file size.
        public string TotalFilesSizeUnit { get; set; } = "octets";
        // Number of files left to copy.
        public int NbFilesLeftToDo { get; set; }
        // Progress percentage (0 to 100).
        public int Progression { get; set; }
        // Timestamp of the last action.
        public string LastActionTimestamp { get; set; }
    }
}