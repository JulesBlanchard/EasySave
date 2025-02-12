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

        // Cette Action permettra de fermer la fenêtre depuis le ViewModel
        public Action? CloseAction { get; set; }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            // On initialise à partir des valeurs existantes
            LogFormat = LoggingManager.LogFormat;
            BusinessSoftwareName = GeneralSettings.BusinessSoftwareName;
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            // Mise à jour des paramètres globaux
            LoggingManager.LogFormat = LogFormat;
            GeneralSettings.BusinessSoftwareName = BusinessSoftwareName;

            // Optionnel : vous pouvez afficher un message de confirmation ici
            // MessageBox.Show("Les paramètres ont été sauvegardés.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);

            // Ferme la fenêtre si une action de fermeture a été assignée
            CloseAction?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}