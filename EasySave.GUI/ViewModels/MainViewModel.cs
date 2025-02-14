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
using EasySave.Utils;
using MessageBox = System.Windows.MessageBox;

namespace EasySave.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private BackupController backupController;
        private ObservableCollection<Backup> allBackups;
        private ObservableCollection<Backup> pagedBackups;
        private int currentPage;
        private int totalPages;
        private readonly int itemsPerPage = 6;
        
        public ICommand ExecuteSelectedCommand { get; }
        public ICommand DeleteSelectedCommand { get; }

        private RelayCommand nextPageCommand;
        private RelayCommand previousPageCommand;
        private RelayCommand lastPageCommand;

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
                if (currentPage != value)
                {
                    currentPage = value;
                    OnPropertyChanged();
                    UpdatePagedBackups();
                    nextPageCommand?.RaiseCanExecuteChanged();
                    previousPageCommand?.RaiseCanExecuteChanged();
                    lastPageCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        
        public int TotalPages
        {
            get => totalPages;
            private set
            {
                if (totalPages != value)
                {
                    totalPages = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Propriété de recherche
        private string searchQuery;
        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                if (searchQuery != value)
                {
                    searchQuery = value;
                    OnPropertyChanged();
                    CurrentPage = 1; // Réinitialise la pagination
                    UpdatePagedBackups();
                }
            }
        }

        public ICommand NextPageCommand => nextPageCommand;
        public ICommand PreviousPageCommand => previousPageCommand;
        public ICommand LaunchCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateBackupCommand { get; }
        public ICommand OpenSettingsCommand { get; } 
        public ICommand OpenEncryptWindowCommand { get; }

        public MainViewModel()
        {
            backupController = BackupController.Instance;
            allBackups = new ObservableCollection<Backup>(backupController.GetBackups());
            pagedBackups = new ObservableCollection<Backup>();

            nextPageCommand = new RelayCommand(NextPage, CanGoNext);
            previousPageCommand = new RelayCommand(PreviousPage, CanGoPrevious);
            lastPageCommand = new RelayCommand(LastPage, CanGoLast);
            
            CurrentPage = 1;

            LaunchCommand = new RelayCommand<Backup>(LaunchBackup);
            EditCommand = new RelayCommand<Backup>(EditBackup);
            DeleteCommand = new RelayCommand<Backup>(DeleteBackup);
            CreateBackupCommand = new RelayCommand(OpenCreateBackup);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenEncryptWindowCommand = new RelayCommand(OpenEncryptWindow);
            
            // Nouvelle commande pour exécuter les sauvegardes sélectionnées
            ExecuteSelectedCommand = new RelayCommand(ExecuteSelectedBackups);
            // Nouvelle commande pour supprimer les sauvegardes sélectionnées
            DeleteSelectedCommand = new RelayCommand(DeleteSelectedBackups);

            backupController.BackupsChanged += RefreshBackups;
            
            SearchQuery = string.Empty;
        }

        private bool CanGoNext() => CurrentPage * itemsPerPage < allBackups.Count;
        private bool CanGoPrevious() => CurrentPage > 1;
        private bool CanGoLast() => CurrentPage < TotalPages;

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
        private void LastPage()
        {
            if (CanGoLast())
                CurrentPage = TotalPages;
        }
        
        /// <summary>
        /// Retourne la liste complète filtrée selon le SearchQuery.
        /// </summary>
        private IEnumerable<Backup> GetFilteredBackups()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return allBackups;
            return allBackups.Where(b => b.Name.IndexOf(SearchQuery, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void RefreshBackups()
        {
            allBackups = new ObservableCollection<Backup>(backupController.GetBackups());
            // On applique le filtre actuel
            CurrentPage = 1;
            UpdatePagedBackups();
        }

        /// <summary>
        /// Met à jour la liste paginée en fonction du filtre et de la page actuelle.
        /// </summary>
        private void UpdatePagedBackups()
        {
            var filtered = GetFilteredBackups().ToList();
            // S'assurer qu'on affiche au moins 1 page
            TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)itemsPerPage));
            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            pagedBackups.Clear();
            var items = filtered.Skip((CurrentPage - 1) * itemsPerPage).Take(itemsPerPage);
            foreach (var item in items)
                pagedBackups.Add(item);
        }
        
        private async void ExecuteSelectedBackups()
        {
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                MessageBox.Show("La sauvegarde ne peut pas être lancée car un logiciel métier est en cours d'exécution.",
                    "Sauvegarde annulée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var selectedBackups = allBackups.Where(b => b.IsSelected).ToList();
            if (!selectedBackups.Any())
            {
                MessageBox.Show("Veuillez sélectionner au moins une sauvegarde.", "Aucun élément sélectionné",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Exécuter chacune des sauvegardes sélectionnées
            foreach (var backup in selectedBackups)
            {
                int index = allBackups.IndexOf(backup);
                await Task.Run(() => backupController.ExecuteBackup(index));
            }

            MessageBox.Show("Les sauvegardes sélectionnées ont été exécutées.", "Succès",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void DeleteSelectedBackups()
        {
            var selectedBackups = allBackups.Where(b => b.IsSelected).ToList();
            if (!selectedBackups.Any())
            {
                MessageBox.Show("Veuillez sélectionner au moins une sauvegarde.", "Aucun élément sélectionné",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Voulez-vous supprimer les sauvegardes sélectionnées ?", "Confirmer la suppression", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Supprimer en commençant par les indices les plus élevés pour éviter les décalages
                var indices = selectedBackups.Select(b => allBackups.IndexOf(b))
                    .OrderByDescending(i => i)
                    .ToList();
                foreach (var index in indices)
                {
                    backupController.DeleteBackup(index);
                }
                RefreshBackups();
                MessageBox.Show("Les sauvegardes sélectionnées ont été supprimées.", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void LaunchBackup(Backup backup)
        {
            // Vérifiez d'abord si un logiciel métier est en cours d'exécution
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                MessageBox.Show(
                    "La sauvegarde ne peut pas être lancée car un logiciel métier est en cours d'exécution.",
                    "Sauvegarde annulée",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

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
            var editWindow = new Views.EditBackupWindow(backup);
            editWindow.ShowDialog();
            RefreshBackups();
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
        
        private void OpenEncryptWindow()
        {
            var encryptWindow = new Views.EncryptFileWindow();
            encryptWindow.ShowDialog();
        }

        private void OpenSettings()
        {
            // Ouvre la fenêtre de réglages (SettingsWindow)
            var settingsWindow = new Views.SettingsWindow();
            settingsWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
