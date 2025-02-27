using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Controllers;
using WpfApp = System.Windows.Application;
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
            // Using singleton
            backupController = BackupController.Instance;
            BackupType = "full";
            CreateBackupCommand = new RelayCommand(CreateBackup);
            CancelCommand = new RelayCommand(Cancel);
            BrowseSourceCommand = new RelayCommand(BrowseSource);
            BrowseTargetCommand = new RelayCommand(BrowseTarget);
        }

        private void CreateBackup()
{
    // Validate required fields
    if (string.IsNullOrWhiteSpace(BackupName))
    {
        MessageBox.Show(
            (string)WpfApp.Current.FindResource("CreateBackup_NameEmpty"),
            (string)WpfApp.Current.FindResource("Common_ValidationError"),
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }
    if (string.IsNullOrWhiteSpace(SourcePath) || !Directory.Exists(SourcePath))
    {
        MessageBox.Show(
            (string)WpfApp.Current.FindResource("CreateBackup_SourceNotExist"),
            (string)WpfApp.Current.FindResource("Common_ValidationError"),
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }
    if (string.IsNullOrWhiteSpace(TargetPath))
    {
        MessageBox.Show(
            (string)WpfApp.Current.FindResource("CreateBackup_TargetEmpty"),
            (string)WpfApp.Current.FindResource("Common_ValidationError"),
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }
    if (ShouldEncrypt && string.IsNullOrWhiteSpace(EncryptionKey))
    {
        MessageBox.Show(
            (string)WpfApp.Current.FindResource("CreateBackup_EmptyEncryptionKey"),
            (string)WpfApp.Current.FindResource("Common_ValidationError"),
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }

    // If the target folder does not exist, ask the user if they want to create it
    if (!Directory.Exists(TargetPath))
    {
        var result = MessageBox.Show(
            string.Format((string)WpfApp.Current.FindResource("CreateBackup_TargetNotExist"), TargetPath),
            (string)WpfApp.Current.FindResource("Common_TargetFolderNotFound"),
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Directory.CreateDirectory(TargetPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format((string)WpfApp.Current.FindResource("CreateBackup_TargetCreationError"), ex.Message),
                    (string)WpfApp.Current.FindResource("Common_Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else
        {
            return;
        }
    }
    // Normalize backup type
    string normalizedType = BackupType.ToLowerInvariant();
    if (normalizedType.Contains("complète"))
    {
        normalizedType = "full";
    }
    else if (normalizedType.Contains("différentielle"))
    {
        normalizedType = "diff";
    }
    backupController.CreateBackup(BackupName, SourcePath, TargetPath, normalizedType, ShouldEncrypt, EncryptionKey);
    MessageBox.Show(
        string.Format((string)WpfApp.Current.FindResource("CreateBackup_Success"), BackupName),
        (string)WpfApp.Current.FindResource("Common_Success"),
        MessageBoxButton.OK, MessageBoxImage.Information);
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
