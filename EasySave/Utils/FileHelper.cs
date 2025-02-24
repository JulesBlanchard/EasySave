using System;
using System.IO;
using EasySave.Models;

namespace EasySave.Utils
{
    public static class FileHelper
    {
        /// <summary>
        /// Copie un fichier par blocs (chunk), vérifie régulièrement Pause/Stop.
        /// </summary>
        /// <param name="sourcePath">Chemin source</param>
        /// <param name="destPath">Chemin de destination</param>
        /// <param name="backup">Backup contenant JobControl (pause/stop)</param>
        public static void CopyFileWithCancellation(string sourcePath, string destPath, Backup backup)
        {
            // Nombre d'octets lus par bloc (par ex. 4 Ko).
            const int bufferSize = 4096;
            var token = backup.JobControl.CancellationToken;

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[bufferSize];
                int bytesRead;
                
                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Vérifier si Stop a été demandé
                    token.ThrowIfCancellationRequested();
                    // Vérifier si Pause a été enclenchée
                    backup.JobControl.WaitIfPaused();

                    // Écrire les octets dans la destination
                    destStream.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}