using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Controllers;
using EasySave.Models;
using EasySave.Utils;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Threading.Tasks;


namespace EasySave.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private BackupController backupController;
        public ObservableCollection<Backup> Backups { get; set; }

        private Backup selectedBackup;
        public Backup SelectedBackup
        {
            get => selectedBackup;
            set { selectedBackup = value; OnPropertyChanged(); }
        }

        private string currentStatus;
        public string CurrentStatus
        {
            get => currentStatus;
            set { currentStatus = value; OnPropertyChanged(); }
        }

        public ICommand OpenCreateBackupCommand { get; }
        public ICommand ExecuteBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand ExecuteAllBackupsCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            backupController = BackupController.Instance;
            Backups = new ObservableCollection<Backup>(backupController.GetBackups());
            // Abonnez-vous à l'événement pour rafraîchir la liste dès qu'elle change
            backupController.BackupsChanged += RefreshBackups;

            OpenCreateBackupCommand = new RelayCommand(OpenCreateBackup);
            ExecuteBackupCommand = new RelayCommand(ExecuteBackup);
            DeleteBackupCommand = new RelayCommand(DeleteBackup);
            ExecuteAllBackupsCommand = new RelayCommand(ExecuteAllBackups);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
            CurrentStatus = "Ready";
        }

        private void RefreshBackups()
        {
            // Comme cette méthode est appelée depuis un thread quelconque, on passe par le Dispatcher
            Application.Current.Dispatcher.Invoke(() =>
            {
                Backups.Clear();
                foreach (var b in backupController.GetBackups())
                {
                    Backups.Add(b);
                }
            });
        }

        private void OpenCreateBackup()
        {
            var createWindow = new Views.CreateBackupWindow();
            createWindow.ShowDialog();
            // La liste sera automatiquement rafraîchie via l'événement BackupsChanged.
            CurrentStatus = "Backup created.";
        }

        private async void ExecuteBackup()
        {
            if (SelectedBackup == null)
            {
                CurrentStatus = "Aucune sauvegarde sélectionnée.";
                return;
            }
            int index = Backups.IndexOf(SelectedBackup);
            try
            {
                await Task.Run(() => backupController.ExecuteBackup(index));
                CurrentStatus = $"La sauvegarde '{SelectedBackup.Name}' a été exécutée.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'exécution de la sauvegarde : " + ex.Message, 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBackup()
        {
            if (SelectedBackup == null)
            {
                CurrentStatus = "Aucune sauvegarde sélectionnée.";
                return;
            }
            // Stocker le nom avant suppression pour le message
            var backupName = SelectedBackup.Name;
            int index = Backups.IndexOf(SelectedBackup);
            backupController.DeleteBackup(index);
            // La liste sera automatiquement rafraîchie via l'événement
            SelectedBackup = null;
            CurrentStatus = $"La sauvegarde '{backupName}' a été supprimée.";
        }

        private void ExecuteAllBackups()
        {
            backupController.ExecuteAllBackups();
            CurrentStatus = "Toutes les sauvegardes ont été exécutées.";
        }

        private void OpenSettings()
        {
            var settingsWindow = new Views.SettingsWindow();
            settingsWindow.ShowDialog();
            CurrentStatus = "Paramètres mis à jour.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
