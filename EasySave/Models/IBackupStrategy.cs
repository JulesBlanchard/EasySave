using EasySave.Logging;

namespace EasySave.Models
{
    /// <summary>
    /// Defines the interface for backup strategies.
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Executes the backup using the specified logger.
        /// </summary>
        /// <param name="backup">The backup job to execute</param>
        /// <param name="logger">Logger to record the backup process</param>
        void Execute(Backup backup, IBackupLogger logger);
    }
}