using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Logging;
using EasySave.Utils;

namespace EasySave.GUI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string logFormat;
        private string businessSoftwareName;
        private string allowedEncryptionFileTypes;

        public string LogFormat
        {
            get => logFormat;
            set { logFormat = value; OnPropertyChanged(); }
        }

        public string BusinessSoftwareName
        {
            get => businessSoftwareName;
            set { businessSoftwareName = value; OnPropertyChanged(); }
        }
        
        public string AllowedEncryptionFileTypes
        {
            get => allowedEncryptionFileTypes;
            set { allowedEncryptionFileTypes = value; OnPropertyChanged(); }
        }

        // Cette Action permettra de fermer la fenêtre depuis le ViewModel
        public Action? CloseAction { get; set; }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            // On initialise à partir des valeurs existantes
            LogFormat = LoggingManager.LogFormat;
            BusinessSoftwareName = GeneralSettings.BusinessSoftwareName;
            AllowedEncryptionFileTypes = GeneralSettings.AllowedEncryptionFileTypes;
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            // Mise à jour des paramètres globaux
            LoggingManager.LogFormat = LogFormat;
            GeneralSettings.BusinessSoftwareName = BusinessSoftwareName;
            GeneralSettings.AllowedEncryptionFileTypes = AllowedEncryptionFileTypes;

            CloseAction?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}