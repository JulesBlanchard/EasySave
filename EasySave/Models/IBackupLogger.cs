using System;

namespace EasySave.Models
{
    public interface IBackupLogger
    {
        /// <summary>
        /// Loggue un transfert de fichier réussi.
        /// </summary>
        /// <param name="backupName">Le nom du Backup</param>
        /// <param name="sourceFile">Chemin complet du fichier source</param>
        /// <param name="destFile">Chemin complet du fichier cible</param>
        /// <param name="fileSize">Taille du fichier en octets</param>
        /// <param name="transferTimeMs">Temps de transfert en millisecondes</param>
        void LogTransfer(
            string backupName,
            string sourceFile,
            string destFile,
            long fileSize,
            long transferTimeMs
        );

        /// <summary>
        /// Loggue une erreur (exception) lors d'un transfert de fichier.
        /// </summary>
        /// <param name="backupName">Le nom du Backup</param>
        /// <param name="sourceFile">Chemin complet du fichier source</param>
        /// <param name="destFile">Chemin complet du fichier cible</param>
        /// <param name="ex">L'exception générée</param>
        void LogError(
            string backupName,
            string sourceFile,
            string destFile,
            Exception ex
        );
    }
}