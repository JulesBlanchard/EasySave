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
    /// TODO : Gérer la localisation (FR/EN) pour tous les messages 
    ///        (console, logs, pop-ups). 
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

            // Nouvelle commande pour exécuter les sauvegardes sélectionnées
            ExecuteSelectedCommand = new RelayCommand(ExecuteSelectedBackups);
            // Nouvelle commande pour supprimer les sauvegardes sélectionnées
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

        /// <summary>
        /// Exécute en parallèle toutes les sauvegardes sélectionnées.
        /// </summary>
private async void ExecuteSelectedBackups()
{
    // Vérifie si un logiciel métier est lancé (vous conservez cette logique si vous le souhaitez)
    if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
    {
        MessageBox.Show(
            "La sauvegarde ne peut pas être lancée car un logiciel métier est en cours d'exécution.",
            "Sauvegarde annulée", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    // Récupère les backups sélectionnées
    var selectedBackups = allBackups.Where(b => b.IsSelected).ToList();
    if (!selectedBackups.Any())
    {
        MessageBox.Show("Veuillez sélectionner au moins une sauvegarde.", "Aucun élément sélectionné",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    // Prépare la liste de tâches
    // Pour chaque backup, on lance ExecuteBackup dans un Task.Run
    var tasks = selectedBackups.Select(backup =>
    {
        // Optionnel : si la sauvegarde est déjà End ou Error, on peut la reset avant
        if (backup.Status == BackupStatus.End || backup.Status == BackupStatus.Error)
        {
            backup.Reset();
        }

        int index = allBackups.IndexOf(backup);
        return Task.Run(() => backupController.ExecuteBackup(index));
    }).ToList();

    try
    {
        // Attend que toutes les sauvegardes se terminent
        await Task.WhenAll(tasks);

        // Si on arrive ici, aucune exception n'a été lancée : toutes les sauvegardes se sont terminées "normalement"
        // On peut alors vérifier si elles ont atteint 100% (pas de Stop au milieu)
        bool allFullProgress = selectedBackups.All(b => b.Progression == 100);

        if (allFullProgress)
        {
            // 100% pour chacune => succès total
            MessageBox.Show("Les sauvegardes sélectionnées ont été exécutées avec succès.", 
                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            // Certaines ont pu s'arrêter à moins de 100% (statut End, 
            // mais progression < 100 => l'utilisateur a peut-être stoppé en cours)
            MessageBox.Show("Certaines sauvegardes se sont terminées sans atteindre 100% (arrêtées ou incomplètes).", 
                "Partiellement exécutées", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    catch (AggregateException agex)
    {
        // S'il y a eu un Stop, OperationCanceledException est levée
        // Task.WhenAll lance une AggregateException pouvant contenir des OperationCanceledException 
        // ou d'autres exceptions.
        if (agex.InnerExceptions.Any(e => e is OperationCanceledException))
        {
            // => au moins un Stop a eu lieu
            MessageBox.Show("Les sauvegardes sélectionnées ont été stoppées par l'utilisateur.", 
                "Annulées", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            // Autres exceptions
            string msg = agex.Flatten().Message;
            MessageBox.Show("Erreur lors de l'exécution : " + msg, 
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    catch (Exception ex)
    {
        // S'il restait une autre exception non gérée
        MessageBox.Show("Erreur lors de l'exécution : " + ex.Message, 
            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
            // Si la sauvegarde est déjà End ou Error, on la réinitialise
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
                // En cas d’annulation (Stop)
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
            // Ouvre la fenêtre de réglages (SettingsWindow)
            var settingsWindow = new Views.SettingsWindow();
            settingsWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private void OnPauseRequested()
        {
            // S'assurer d'être sur le thread UI
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

            // Assurez-vous que Backup.Status est bien mis à jour dans vos stratégies.
            foreach (var backup in allBackups.Where(b => b.Status == BackupStatus.Active))
            {
                progressVM.RunningBackups.Add(backup);
            }
            progressWindow.DataContext = progressVM;
            progressWindow.Owner = Application.Current.MainWindow;
            progressWindow.ShowDialog();
        }
    }
}
