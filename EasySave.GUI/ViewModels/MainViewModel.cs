using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EasySave.Controllers;
using EasySave.Models;
using MessageBox = System.Windows.MessageBox;

namespace EasySave.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private BackupController backupController;
        private ObservableCollection<Backup> allBackups;
        private ObservableCollection<Backup> pagedBackups;
        private int currentPage;
        private readonly int itemsPerPage = 6;

        // Commandes RelayCommand pour la pagination
        private RelayCommand nextPageCommand;
        private RelayCommand previousPageCommand;

        public ObservableCollection<Backup> PagedBackups
        {
            get => pagedBackups;
            set { pagedBackups = value; OnPropertyChanged(); }
        }

        public int CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                OnPropertyChanged();
                UpdatePagedBackups();
                // Vérifier si les commandes sont instanciées avant de les utiliser
                nextPageCommand?.RaiseCanExecuteChanged();
                previousPageCommand?.RaiseCanExecuteChanged();
            }
        }

        public ICommand NextPageCommand => nextPageCommand;
        public ICommand PreviousPageCommand => previousPageCommand;
        public ICommand LaunchCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateBackupCommand { get; }

        public MainViewModel()
        {
            backupController = BackupController.Instance;
            allBackups = new ObservableCollection<Backup>(backupController.GetBackups());
            pagedBackups = new ObservableCollection<Backup>();

            // Initialiser d'abord les commandes de pagination
            nextPageCommand = new RelayCommand(NextPage, CanGoNext);
            previousPageCommand = new RelayCommand(PreviousPage, CanGoPrevious);

            // Affecter CurrentPage après que les commandes soient initialisées
            CurrentPage = 1;

            LaunchCommand = new RelayCommand<Backup>(LaunchBackup);
            EditCommand = new RelayCommand<Backup>(EditBackup);
            DeleteCommand = new RelayCommand<Backup>(DeleteBackup);
            CreateBackupCommand = new RelayCommand(OpenCreateBackup);

            backupController.BackupsChanged += RefreshBackups;
        }

        private bool CanGoNext() => CurrentPage * itemsPerPage < allBackups.Count;
        private bool CanGoPrevious() => CurrentPage > 1;

        private void NextPage()
        {
            if (CanGoNext())
                CurrentPage++;
        }

        private void PreviousPage()
        {
            if (CanGoPrevious())
                CurrentPage--;
        }

        private void RefreshBackups()
        {
            allBackups = new ObservableCollection<Backup>(backupController.GetBackups());
            if ((CurrentPage - 1) * itemsPerPage >= allBackups.Count)
                CurrentPage = 1;
            else
                UpdatePagedBackups();
        }

        private void UpdatePagedBackups()
        {
            pagedBackups.Clear();
            var items = allBackups.Skip((CurrentPage - 1) * itemsPerPage).Take(itemsPerPage);
            foreach (var item in items)
                pagedBackups.Add(item);
        }

        private async void LaunchBackup(Backup backup)
        {
            try
            {
                int index = allBackups.IndexOf(backup);
                await Task.Run(() => backupController.ExecuteBackup(index));
                MessageBox.Show($"La sauvegarde '{backup.Name}' a été exécutée.", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'exécution : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditBackup(Backup backup)
        {
            MessageBox.Show($"Fonction d'édition pour la sauvegarde '{backup.Name}' (à implémenter).");
        }

        private void DeleteBackup(Backup backup)
        {
            int index = allBackups.IndexOf(backup);
            backupController.DeleteBackup(index);
            RefreshBackups();
            MessageBox.Show($"La sauvegarde '{backup.Name}' a été supprimée.", "Succès",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenCreateBackup()
        {
            var createWindow = new Views.CreateBackupWindow();
            createWindow.ShowDialog();
            RefreshBackups();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
