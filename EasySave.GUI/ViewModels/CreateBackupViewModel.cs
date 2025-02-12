using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Controllers;
using EasySave.Models;
using EasySave.Utils;

namespace EasySave.GUI.ViewModels
{
    public class CreateBackupViewModel : INotifyPropertyChanged
    {
        private BackupController backupController;
        private Window window;

        public string WindowTitle { get; set; }
        public string BackupNameLabel { get; set; }
        public string SourcePathLabel { get; set; }
        public string TargetPathLabel { get; set; }
        public string BackupTypeLabel { get; set; }
        public string CreateButtonText { get; set; }
        public string CancelButtonText { get; set; }

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

        public ICommand CreateBackupCommand { get; }
        public ICommand CancelCommand { get; }

        public CreateBackupViewModel(Window window)
        {
            this.window = window;
            backupController = new BackupController();

            // Initialize labels from LocalizationManager if defined, or use default strings.
            WindowTitle = LocalizationManager.CurrentMessages.ContainsKey("CreateBackupWindow_Title")
                ? LocalizationManager.CurrentMessages["CreateBackupWindow_Title"]
                : "Create Backup";
            BackupNameLabel = LocalizationManager.CurrentMessages.ContainsKey("EnterBackupName")
                ? LocalizationManager.CurrentMessages["EnterBackupName"]
                : "Backup Name: ";
            SourcePathLabel = LocalizationManager.CurrentMessages.ContainsKey("EnterSourcePath")
                ? LocalizationManager.CurrentMessages["EnterSourcePath"]
                : "Source Path: ";
            TargetPathLabel = LocalizationManager.CurrentMessages.ContainsKey("EnterTargetPath")
                ? LocalizationManager.CurrentMessages["EnterTargetPath"]
                : "Target Path: ";
            BackupTypeLabel = LocalizationManager.CurrentMessages.ContainsKey("EnterBackupType")
                ? LocalizationManager.CurrentMessages["EnterBackupType"]
                : "Backup Type (full/diff): ";
            CreateButtonText = LocalizationManager.CurrentMessages.ContainsKey("CreateBackupWindow_CreateButton")
                ? LocalizationManager.CurrentMessages["CreateBackupWindow_CreateButton"]
                : "Create";
            CancelButtonText = LocalizationManager.CurrentMessages.ContainsKey("CreateBackupWindow_CancelButton")
                ? LocalizationManager.CurrentMessages["CreateBackupWindow_CancelButton"]
                : "Cancel";

            // Default value for backup type.
            BackupType = "full";

            CreateBackupCommand = new RelayCommand(CreateBackup);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void CreateBackup()
        {
            if (string.IsNullOrWhiteSpace(BackupName))
            {
                MessageBox.Show("Backup name cannot be empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SourcePath) || !Directory.Exists(SourcePath))
            {
                MessageBox.Show("Source directory does not exist.");
                return;
            }
            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                MessageBox.Show("Target directory cannot be empty.");
                return;
            }
            if (!Directory.Exists(TargetPath))
            {
                try
                {
                    Directory.CreateDirectory(TargetPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating target directory: " + ex.Message);
                    return;
                }
            }

            backupController.CreateBackup(BackupName, SourcePath, TargetPath, BackupType);
            MessageBox.Show($"Backup '{BackupName}' created successfully.");
            window.Close();
        }

        private void Cancel()
        {
            window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
