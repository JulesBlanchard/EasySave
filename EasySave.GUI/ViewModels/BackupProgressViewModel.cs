using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Models;

namespace EasySave.GUI.ViewModels
{
    public class BackupProgressViewModel : INotifyPropertyChanged
    {
        // Collection des sauvegardes en cours (à mettre à jour en temps réel)
        public ObservableCollection<Backup> RunningBackups { get; set; } = new ObservableCollection<Backup>();

        // Commandes de contrôle pour chaque sauvegarde
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