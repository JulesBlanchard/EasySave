using System;
using System.Collections.Generic;
using EasySave.Logging;

namespace EasySave.Tests.Utils
{
    public class FakeLogger : IBackupLogger
    {
        public List<string> Logs = new List<string>();

        public void LogTransfer(string backupName, string sourceFile, string destFile, long fileSize, long transferTimeMs)
        {
            Logs.Add($"Transfer: {backupName}");
        }

        public void LogError(string backupName, string sourceFile, string destFile, Exception ex)
        {
            Logs.Add($"Error: {backupName}");
        }

        public void LogEncryption(string filePath, int encryptionTime)
        {
            Logs.Add($"Encryption: {filePath}");
        }
    }
}