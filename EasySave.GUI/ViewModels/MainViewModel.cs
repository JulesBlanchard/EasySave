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
using Application = System.Windows.Application;
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
            set
            {
                pagedBackups = value;
                OnPropertyChanged();
            }
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
                    CurrentPage = 1; 
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
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ShowProgressPopupCommand { get; }


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

            ExecuteSelectedCommand = new RelayCommand(ExecuteSelectedBackups);
            DeleteSelectedCommand = new RelayCommand(DeleteSelectedBackups);

            backupController.BackupsChanged += RefreshBackups;

            SearchQuery = string.Empty;
            
            PauseNotifierEvent.PauseRequested += OnPauseRequested;

            PauseCommand = new RelayCommand<Backup>(backup => backup.JobControl.Pause(backup));
            ResumeCommand = new RelayCommand<Backup>(backup => backup.JobControl.Resume(backup));
            StopCommand = new RelayCommand<Backup>(backup => backup.JobControl.Stop(backup));
            ShowProgressPopupCommand = new RelayCommand(OpenBackupProgressWindow);
            backupController.BackupsChanged += RefreshBackups;


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
        /// Returns the complete list filtered according to the SearchQuery.
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
            // Apply the current filter
            CurrentPage = 1;
            UpdatePagedBackups();
        }

        /// <summary>
        /// Updates the paginated list based on the filter and the current page.
        /// </summary>
        private void UpdatePagedBackups()
        {
            var filtered = GetFilteredBackups().ToList();
            // Ensure at least one page is displayed
            TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)itemsPerPage));
            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            pagedBackups.Clear();
            var items = filtered.Skip((CurrentPage - 1) * itemsPerPage).Take(itemsPerPage);
            foreach (var item in items)
                pagedBackups.Add(item);
        }

        /// <summary>
        /// Executes all selected backups in parallel.
        /// </summary>
        private async void ExecuteSelectedBackups()
        {
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                MessageBox.Show(
                    (string)Application.Current.FindResource("Main_JobRunning"),
                    (string)Application.Current.FindResource("Common_Warning"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedBackups = allBackups.Where(b => b.IsSelected).ToList();
            if (!selectedBackups.Any())
            {
                MessageBox.Show(
                    (string)Application.Current.FindResource("Main_NoSelection"),
                    (string)Application.Current.FindResource("Common_Warning"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            var tasks = selectedBackups.Select(backup =>
            {
                
                if (backup.Status == BackupStatus.End || backup.Status == BackupStatus.Error)
                {
                    backup.Reset();
                }

                int index = allBackups.IndexOf(backup);
                return Task.Run(() => backupController.ExecuteBackup(index));
            }).ToList();

            try
            {
                await Task.WhenAll(tasks);
                bool allFullProgress = selectedBackups.All(b => b.Progression == 100);

                if (allFullProgress)
                {
                    MessageBox.Show(
                        (string)Application.Current.FindResource("Main_SuccessAll"),
                        (string)Application.Current.FindResource("Common_Success"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        (string)Application.Current.FindResource("Main_PartialSuccess"),
                        (string)Application.Current.FindResource("Common_Information"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (AggregateException agex)
            {
                if (agex.InnerExceptions.Any(e => e is OperationCanceledException))
                {
                    MessageBox.Show(
                        (string)Application.Current.FindResource("Main_Stopped"),
                        (string)Application.Current.FindResource("Common_Information"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string msg = agex.Flatten().Message;
                    MessageBox.Show(
                        string.Format((string)Application.Current.FindResource("Main_ExecutionError"), msg),
                        (string)Application.Current.FindResource("Common_Error"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format((string)Application.Current.FindResource("Main_ExecutionError"), ex.Message),
                    (string)Application.Current.FindResource("Common_Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            if (backup.Status == BackupStatus.End || backup.Status == BackupStatus.Error)
            {
                backup.Reset();
            }

            try
            {
                int index = allBackups.IndexOf(backup);
                await Task.Run(() => backupController.ExecuteBackup(index));
                MessageBox.Show($"La sauvegarde '{backup.Name}' est terminée avec succès.", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show($"La sauvegarde '{backup.Name}' a été arrêtée.", 
                    "Annulée", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'exécution : " + ex.Message, 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var settingsWindow = new Views.SettingsWindow();
            settingsWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private void OnPauseRequested()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Les sauvegardes sont en pause car le logiciel métier est en cours d'exécution.\nElles reprendront dès que le logiciel sera fermé.",
                    "Sauvegardes en pause", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        
        private void OpenBackupProgressWindow()
        {
            var progressWindow = new Views.BackupProgressWindow();
            var progressVM = new BackupProgressViewModel();

            foreach (var backup in allBackups.Where(b => b.Status == BackupStatus.Active || b.Status == BackupStatus.Paused))
            {
                progressVM.RunningBackups.Add(backup);
            }
            progressWindow.DataContext = progressVM;
            progressWindow.Owner = Application.Current.MainWindow;
            progressWindow.ShowDialog();
        }
    }
}
