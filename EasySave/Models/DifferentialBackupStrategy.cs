namespace EasySave.Models
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(Backup backup)
        {
            // TODO: Implémentation d'une sauvegarde différentielle
            // Ex: Ne copier que les fichiers nouveaux ou modifiés
        }
    }
}