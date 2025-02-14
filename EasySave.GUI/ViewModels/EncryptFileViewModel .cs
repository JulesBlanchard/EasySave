using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Logging;

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
                MessageBox.Show("Veuillez sélectionner un fichier valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(Key))
            {
                MessageBox.Show("La clé de cryptage ne peut pas être vide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            try
            {
                // Utilisation de la classe FileManager de CryptoSoft pour crypter le fichier
                var fileManager = new CryptoSoft.FileManager(FilePath, Key);
                int encryptionTime = fileManager.TransformFile();
                
                // Récupération du logger configuré
                IBackupLogger logger = LoggingManager.GetLogger("Logs");
                logger.LogEncryption(FilePath, encryptionTime);
                
                // Afficher un message en fonction du résultat
                if(encryptionTime < 0)
                    MessageBox.Show($"Le cryptage a échoué (code erreur {encryptionTime}).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                else if(encryptionTime == 0)
                    MessageBox.Show("Aucun cryptage n'a été effectué.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show($"Cryptage effectué en {encryptionTime} ms.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Erreur lors du cryptage : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
