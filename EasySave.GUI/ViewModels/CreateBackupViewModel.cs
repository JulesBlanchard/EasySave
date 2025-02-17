using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Controllers;
using EasySave.Utils;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox; // Pour FolderBrowserDialog

namespace EasySave.GUI.ViewModels
{
    public class CreateBackupViewModel : INotifyPropertyChanged
    {
        private BackupController backupController;
        private Window window;

        private string backupName;
        public string BackupName
        {
            get => backupName;
            set { backupName = value; OnPropertyChanged(); }
        }

        private string sourcePath;
        public string SourcePath
        {
            get => sourcePath;
            set { sourcePath = value; OnPropertyChanged(); }
        }

        private string targetPath;
        public string TargetPath
        {
            get => targetPath;
            set { targetPath = value; OnPropertyChanged(); }
        }

        private string backupType;
        public string BackupType
        {
            get => backupType;
            set { backupType = value; OnPropertyChanged(); }
        }
        
        private bool shouldEncrypt;
        public bool ShouldEncrypt
        {
            get => shouldEncrypt;
            set { shouldEncrypt = value; OnPropertyChanged(); }
        }

        private string encryptionKey;
        public string EncryptionKey
        {
            get => encryptionKey;
            set { encryptionKey = value; OnPropertyChanged(); }
        }

        public ICommand CreateBackupCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseTargetCommand { get; }

        public CreateBackupViewModel(Window window)
        {
            this.window = window;
            // Utilisation du singleton
            backupController = BackupController.Instance;
            BackupType = "full";
            CreateBackupCommand = new RelayCommand(CreateBackup);
            CancelCommand = new RelayCommand(Cancel);
            BrowseSourceCommand = new RelayCommand(BrowseSource);
            BrowseTargetCommand = new RelayCommand(BrowseTarget);
        }

        private void CreateBackup()
        {
            // Vérification des champs obligatoires
            if (string.IsNullOrWhiteSpace(BackupName))
            {
                MessageBox.Show("Le nom de la sauvegarde ne peut pas être vide.", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(SourcePath) || !Directory.Exists(SourcePath))
            {
                MessageBox.Show("Le dossier source n'existe pas.", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                MessageBox.Show("Le dossier cible ne peut pas être vide.", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ShouldEncrypt && string.IsNullOrWhiteSpace(EncryptionKey))
            {
                MessageBox.Show("La clé de chiffrement ne peut pas être vide si le cryptage est activé.", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Si le dossier cible n'existe pas, proposer de le créer
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

            backupController.CreateBackup(BackupName, SourcePath, TargetPath, BackupType, ShouldEncrypt, EncryptionKey);
            MessageBox.Show($"La sauvegarde '{BackupName}' a été créée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
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
