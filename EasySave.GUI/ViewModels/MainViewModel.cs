using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Controllers;
using EasySave.Models;
using EasySave.Utils;

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

        public ICommand CreateBackupCommand { get; }
        public ICommand ExecuteBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand ExecuteAllBackupsCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            backupController = new BackupController();
            // Initialize the backups collection from the controller.
            Backups = new ObservableCollection<Backup>(backupController.GetBackups());
            CreateBackupCommand = new RelayCommand(CreateBackup);
            ExecuteBackupCommand = new RelayCommand(ExecuteBackup);
            DeleteBackupCommand = new RelayCommand(DeleteBackup);
            ExecuteAllBackupsCommand = new RelayCommand(ExecuteAllBackups);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
            CurrentStatus = "Ready";
        }

        private void CreateBackup()
        {
            // Open the CreateBackupWindow to let the user enter backup details.
            var createWindow = new Views.CreateBackupWindow();
            createWindow.ShowDialog();
            // Refresh the backups collection after creation.
            Backups.Clear();
            foreach (var b in backupController.GetBackups())
            {
                Backups.Add(b);
            }
            CurrentStatus = "Backup created.";
        }


        private void ExecuteBackup()
        {
            if (SelectedBackup == null)
            {
                CurrentStatus = "No backup selected.";
                return;
            }
            int index = Backups.IndexOf(SelectedBackup);
            backupController.ExecuteBackup(index);
            CurrentStatus = $"Backup {SelectedBackup.Name} executed.";
        }

        private void DeleteBackup()
        {
            if (SelectedBackup == null)
            {
                CurrentStatus = "No backup selected.";
                return;
            }
            int index = Backups.IndexOf(SelectedBackup);
            backupController.DeleteBackup(index);
            Backups.Remove(SelectedBackup);
            CurrentStatus = $"Backup {SelectedBackup.Name} deleted.";
        }

        private void ExecuteAllBackups()
        {
            backupController.ExecuteAllBackups();
            CurrentStatus = "All backups executed.";
        }

        private void OpenSettings()
        {
            var settingsWindow = new Views.SettingsWindow();
            settingsWindow.ShowDialog();
            CurrentStatus = "Settings updated.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
