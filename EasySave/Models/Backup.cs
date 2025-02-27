using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using EasySave.Logging;

namespace EasySave.Models
{
    /// <summary>
    /// Represents a backup job with a name, source/target paths, and a backup type.
    /// </summary>
    public class Backup : INotifyPropertyChanged 
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string BackupType { get; set; }
        public BackupStatus status { get; set; } = BackupStatus.NotStarted;

        private int progression;

        private bool isSelected;
        
        public bool ShouldEncrypt { get; set; }
        public string EncryptionKey { get; set; }
        [JsonIgnore] // Don't serialize JobControl
        public BackupJobControl JobControl { get; set; } = new BackupJobControl();
        public BackupStatus Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int Progression
        {
            get => progression;
            set
            {
                if (progression != value)
                {
                    progression = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// Default constructor for deserialization.
        /// </summary>
        public Backup() { }

        /// <summary>
        /// Constructor used for creating a backup in code.
        /// </summary>
        public Backup(string name, string source, string target, string backupType = null)
        {
            Name = name;
            SourcePath = source;
            TargetPath = target;
            BackupType = backupType;
        }
        
        
        /// <summary>
        /// Property used for selection in the interface. 
        /// It will not be serialized.
        /// </summary>
        [JsonIgnore]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Not serialized, as the strategy is reinitialized when loading.
        [JsonIgnore]
        public IBackupStrategy Strategy { get; set; }
        
        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Returns the list of files in the source directory (including subdirectories).
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
        /// Executes the backup using the specified logger and the chosen strategy.
        /// </summary>
        public void Execute(IBackupLogger logger)
        {
            if (Strategy != null)
            {
                Strategy.Execute(this, logger);
            }
            else
            {
                Console.WriteLine($"[Backup] No strategy defined for '{Name}'. Nothing to do.");
            }
        }
        
        public void Reset()
        {
            // Recreates a new JobControl (thus a new CancellationToken)
            JobControl = new BackupJobControl();
    
            // Resets progress and status to zero.
            Progression = 0;
            Status = BackupStatus.NotStarted;
        }


    }
}
