using System;

namespace EasySave.Logging
{
    /// <summary>
    /// Defines the logging methods for backup operations.
    /// </summary>
    public interface IBackupLogger
    {
        /// <summary>
        /// Logs a successful file transfer.
        /// </summary>
        /// <param name="backupName">Backup name</param>
        /// <param name="sourceFile">Full source file path</param>
        /// <param name="destFile">Full destination file path</param>
        /// <param name="fileSize">File size in bytes</param>
        /// <param name="transferTimeMs">Transfer time in milliseconds</param>
        void LogTransfer(
            string backupName,
            string sourceFile,
            string destFile,
            long fileSize,
            long transferTimeMs
        );

        /// <summary>
        /// Logs an error that occurs during a file transfer.
        /// </summary>
        /// <param name="backupName">Backup name</param>
        /// <param name="sourceFile">Full source file path</param>
        /// <param name="destFile">Full destination file path</param>
        /// <param name="ex">The exception that was thrown</param>
        void LogError(
            string backupName,
            string sourceFile,
            string destFile,
            Exception ex
        );
        
        void LogEncryption(string filePath, int encryptionTime);
    }
}