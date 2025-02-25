using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Logging;
using EasySave.Utils;
using WpfApp = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace EasySave.GUI.ViewModels
{
    public class EncryptFileViewModel : INotifyPropertyChanged
    {
        private string filePath;
        public string FilePath
        {
            get => filePath;
            set { filePath = value; OnPropertyChanged(); }
        }
        
        private string key;
        public string Key
        {
            get => key;
            set { key = value; OnPropertyChanged(); }
        }
        
        public ICommand BrowseFileCommand { get; }
        public ICommand EncryptCommand { get; }
        public ICommand CancelCommand { get; }
        
        // Pour fermer la fenêtre depuis le ViewModel
        public Action CloseAction { get; set; }
        
        public EncryptFileViewModel()
        {
            BrowseFileCommand = new RelayCommand(BrowseFile);
            EncryptCommand = new RelayCommand(EncryptFile);
            CancelCommand = new RelayCommand(() => CloseAction?.Invoke());
        }
        
        private void BrowseFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }
        
        private void EncryptFile()
{
    if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
    {
        MessageBox.Show(
            (string)WpfApp.Current.FindResource("EncryptFile_InvalidFile"),
            (string)WpfApp.Current.FindResource("Common_Error"),
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }
    if (string.IsNullOrWhiteSpace(Key))
    {
        MessageBox.Show(
            (string)WpfApp.Current.FindResource("EncryptFile_EmptyKey"),
            (string)WpfApp.Current.FindResource("Common_Error"),
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }

    // Vérification de l'extension autorisée
    string allowedTypes = GeneralSettings.AllowedEncryptionFileTypes;
    var allowedExtensions = allowedTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(ext => ext.Trim().ToLowerInvariant())
                                        .ToList();
    string fileExtension = System.IO.Path.GetExtension(FilePath).ToLowerInvariant();
    if (!allowedExtensions.Contains(fileExtension))
    {
        MessageBox.Show(
            string.Format((string)WpfApp.Current.FindResource("EncryptFile_NotAllowedType"), fileExtension),
            (string)WpfApp.Current.FindResource("Common_Error"),
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }

    try
    {
        var fileManager = new CryptoSoft.FileManager(FilePath, Key);
        int encryptionTime = fileManager.TransformFile();
        IBackupLogger logger = LoggingManager.GetLogger("Logs");
        logger.LogEncryption(FilePath, encryptionTime);

        if (encryptionTime < 0)
        {
            MessageBox.Show(
                string.Format((string)WpfApp.Current.FindResource("EncryptFile_Failure"), encryptionTime),
                (string)WpfApp.Current.FindResource("Common_Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else if (encryptionTime == 0)
        {
            MessageBox.Show(
                (string)WpfApp.Current.FindResource("EncryptFile_NoEncryption"),
                (string)WpfApp.Current.FindResource("Common_Information"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(
                string.Format((string)WpfApp.Current.FindResource("EncryptFile_Success"), encryptionTime),
                (string)WpfApp.Current.FindResource("Common_Success"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    catch(Exception ex)
    {
        MessageBox.Show(
            string.Format((string)WpfApp.Current.FindResource("EncryptFile_Error"), ex.Message),
            (string)WpfApp.Current.FindResource("Common_Error"),
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
    finally
    {
        CloseAction?.Invoke();
    }
}


        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
