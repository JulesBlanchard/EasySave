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

        // The token to signal termination
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        // Pause the job and set Status = Paused
        public void Pause(Backup backup)
        {
            backup.Status = BackupStatus.Paused;
            pauseEvent.Reset();
        }

        // Resume the job and set Status = Active
        public void Resume(Backup backup)
        {
            backup.Status = BackupStatus.Active;
            pauseEvent.Set();
        }

        // Immediately stop the job, set Status = End, cancel the token
        public void Stop(Backup backup)
        {
            backup.Status = BackupStatus.End;
            cancellationTokenSource.Cancel();
            pauseEvent.Set(); // In case of pause, release so the thread can see the cancellation
        }

        // Waiting method: blocks if paused
        public void WaitIfPaused() => pauseEvent.Wait();

        // Indicates if the job is paused
        public bool IsPaused => !pauseEvent.IsSet;
    }
}