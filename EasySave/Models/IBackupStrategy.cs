namespace EasySave.Models
{
    public interface IBackupStrategy
    {
        /// <summary>
        /// Exécute la sauvegarde pour le Backup spécifié.
        /// </summary>
        /// <param name="backup">Référence à l'objet Backup</param>
        void Execute(Backup backup);
    }
}