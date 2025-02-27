using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Utils
{
    public static class FileHelper
    {
        /// <summary>
        /// Copies a file in chunks, regularly checking for Pause/Stop.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destPath">Destination path</param>
        /// <param name="backup">Backup containing JobControl (pause/stop)</param>
        public static void CopyFileWithCancellation(string sourcePath, string destPath, Backup backup)
        {
            // Number of bytes read per chunk (e.g., 4 KB).
            const int bufferSize = 4096;
            var token = backup.JobControl.CancellationToken;

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[bufferSize];
                int bytesRead;
                
                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Check if Stop was requested
                    token.ThrowIfCancellationRequested();
                    // Check if Pause was triggered
                    backup.JobControl.WaitIfPaused();

                    // Write bytes to the destination
                    destStream.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}