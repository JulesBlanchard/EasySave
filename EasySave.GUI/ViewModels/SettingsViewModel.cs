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
        private string priorityExtensions; // Nouvelle propriété

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

        // Propriété pour les extensions prioritaires
        public string PriorityExtensions
        {
            get => priorityExtensions;
            set { priorityExtensions = value; OnPropertyChanged(); }
        }

        // Action pour fermer la fenêtre depuis le ViewModel
        public System.Action CloseAction { get; set; }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            // Initialiser les valeurs à partir de GeneralSettings
            LogFormat = LoggingManager.LogFormat;
            BusinessSoftwareName = GeneralSettings.BusinessSoftwareName;
            AllowedEncryptionFileTypes = GeneralSettings.AllowedEncryptionFileTypes;
            PriorityExtensions = GeneralSettings.PriorityExtensions;  // Initialisation de la nouvelle propriété

            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            // Mettre à jour les paramètres globaux
            LoggingManager.LogFormat = LogFormat;
            GeneralSettings.BusinessSoftwareName = BusinessSoftwareName;
            GeneralSettings.AllowedEncryptionFileTypes = AllowedEncryptionFileTypes;
            GeneralSettings.PriorityExtensions = PriorityExtensions;  // Mise à jour des extensions prioritaires

            CloseAction?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
