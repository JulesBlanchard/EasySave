using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Models;

namespace EasySave.GUI.ViewModels
{
    public class BackupProgressViewModel : INotifyPropertyChanged
    {
        // Collection of running backups (to be updated in real-time)
        public ObservableCollection<Backup> RunningBackups { get; set; } = new ObservableCollection<Backup>();

        // Control commands for each backup
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand { get; }

        public BackupProgressViewModel()
        {
            PauseCommand = new RelayCommand<Backup>(backup => backup.JobControl.Pause(backup));
            ResumeCommand = new RelayCommand<Backup>(backup => backup.JobControl.Resume(backup));
            StopCommand = new RelayCommand<Backup>(backup => backup.JobControl.Stop(backup));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}