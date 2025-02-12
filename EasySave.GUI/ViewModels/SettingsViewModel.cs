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

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            LogFormat = LoggingManager.LogFormat;
            BusinessSoftwareName = GeneralSettings.BusinessSoftwareName;
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            LoggingManager.LogFormat = LogFormat;
            GeneralSettings.BusinessSoftwareName = BusinessSoftwareName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}