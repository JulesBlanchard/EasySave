using System.Threading;

namespace EasySave.Models
{
    public class BackupJobControl
    {
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEventSlim pauseEvent;

        public BackupJobControl()
        {
            cancellationTokenSource = new CancellationTokenSource();
            pauseEvent = new ManualResetEventSlim(true); // signalé (non en pause) par défaut
        }

        // Le token pour signaler l’arrêt
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        // Mettre en pause le travail et passer Status = Paused
        public void Pause(Backup backup)
        {
            backup.Status = BackupStatus.Paused;
            pauseEvent.Reset();
        }

        // Reprendre le travail et passer Status = Active
        public void Resume(Backup backup)
        {
            backup.Status = BackupStatus.Active;
            pauseEvent.Set();
        }

        // Stopper immédiatement le travail, Status = End, annuler le token
        public void Stop(Backup backup)
        {
            backup.Status = BackupStatus.End;
            cancellationTokenSource.Cancel();
            pauseEvent.Set(); // en cas de pause, on libère pour que le thread puisse voir l'annulation
        }

        // Méthode d’attente : bloque si en pause
        public void WaitIfPaused() => pauseEvent.Wait();

        // Indique si le travail est en pause
        public bool IsPaused => !pauseEvent.IsSet;
    }
}