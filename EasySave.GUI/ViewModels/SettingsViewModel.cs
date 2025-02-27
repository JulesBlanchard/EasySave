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
        private string priorityExtensions; 
        private string maxLargeFileSize; 


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

        public string PriorityExtensions
        {
            get => priorityExtensions;
            set { priorityExtensions = value; OnPropertyChanged(); }
        }
        
        public string MaxLargeFileSize
        {
            get => maxLargeFileSize;
            set { maxLargeFileSize = value; OnPropertyChanged(); }
        }

        public System.Action CloseAction { get; set; }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            LogFormat = LoggingManager.LogFormat;
            BusinessSoftwareName = GeneralSettings.BusinessSoftwareName;
            AllowedEncryptionFileTypes = GeneralSettings.AllowedEncryptionFileTypes;
            PriorityExtensions = GeneralSettings.PriorityExtensions;  // Initialisation de la nouvelle propriété
            MaxLargeFileSize = GeneralSettings.MaxLargeFileSize.ToString();
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            LoggingManager.LogFormat = LogFormat;
            GeneralSettings.BusinessSoftwareName = BusinessSoftwareName;
            GeneralSettings.AllowedEncryptionFileTypes = AllowedEncryptionFileTypes;
            GeneralSettings.PriorityExtensions = PriorityExtensions;  // Mise à jour des extensions prioritaires
            if (long.TryParse(MaxLargeFileSize, out long parsedSize))
            {
                GeneralSettings.MaxLargeFileSize = parsedSize;
            }
            else
            {
                GeneralSettings.MaxLargeFileSize = (2L * 1024 * 1024 * 1024);
            }
            CloseAction?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
