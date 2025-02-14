using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Controllers;
using EasySave.Models;
using System.Windows.Forms; // Pour FolderBrowserDialog
using MessageBox = System.Windows.MessageBox;

namespace EasySave.GUI.ViewModels
{
    public class EditBackupViewModel : INotifyPropertyChanged
    {
        private Backup backup;
        private BackupController backupController;
        private Window window;

        public string BackupName
        {
            get => backup.Name;
            set { backup.Name = value; OnPropertyChanged(); }
        }

        public string SourcePath
        {
            get => backup.SourcePath;
            set { backup.SourcePath = value; OnPropertyChanged(); }
        }

        public string TargetPath
        {
            get => backup.TargetPath;
            set { backup.TargetPath = value; OnPropertyChanged(); }
        }

        public string BackupType
        {
            get => backup.BackupType;
            set { backup.BackupType = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseTargetCommand { get; }

        public EditBackupViewModel(Backup backup, Window window)
        {
            this.backup = backup;
            this.window = window;
            backupController = BackupController.Instance;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            BrowseSourceCommand = new RelayCommand(BrowseSource);
            BrowseTargetCommand = new RelayCommand(BrowseTarget);
        }

        private void Save()
        {
            // Vérifier que les champs ne sont pas vides
            if (string.IsNullOrWhiteSpace(BackupName))
            {
                MessageBox.Show("Le nom de la sauvegarde ne peut pas être vide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(SourcePath) || !Directory.Exists(SourcePath))
            {
                MessageBox.Show("Le dossier source n'existe pas.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                MessageBox.Show("Le dossier cible ne peut pas être vide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Directory.Exists(TargetPath))
            {
                var result = MessageBox.Show($"Le dossier cible '{TargetPath}' n'existe pas. Voulez-vous le créer ?", 
                    "Dossier non trouvé", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(TargetPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors de la création du dossier cible : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            // Mise à jour de la stratégie en fonction du type choisi
            if (!string.IsNullOrEmpty(BackupType))
            {
                if (BackupType.ToLower().StartsWith("f"))
                    backup.Strategy = new FullBackupStrategy();
                else
                    backup.Strategy = new DifferentialBackupStrategy();
            }

            // Mise à jour via le contrôleur
            backupController.UpdateBackup(backup);

            MessageBox.Show($"La sauvegarde '{BackupName}' a été modifiée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            window.Close();
        }

        private void Cancel()
        {
            window.Close();
        }

        private void BrowseSource()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Sélectionnez le dossier source";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SourcePath = dialog.SelectedPath;
                }
            }
        }

        private void BrowseTarget()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Sélectionnez le dossier cible";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    TargetPath = dialog.SelectedPath;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
