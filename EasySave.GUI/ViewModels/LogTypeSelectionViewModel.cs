using System.Windows.Input;
using EasySave.Logging;
using EasySave.GUI.Views;

namespace EasySave.GUI.ViewModels
{
    public class LogTypeSelectionViewModel
    {
        private readonly LogTypeSelectionWindow window;

        public ICommand SelectXmlCommand { get; }
        public ICommand SelectJsonCommand { get; }

        public LogTypeSelectionViewModel(LogTypeSelectionWindow window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window), "La fenêtre de sélection du type de log est null.");

            this.window = window;
            SelectXmlCommand = new RelayCommand(SelectXml);
            SelectJsonCommand = new RelayCommand(SelectJson);
        }

        private void SelectXml()
        {
            LoggingManager.LogFormat = "XML";
            var mainWindow = new MainWindow();
            mainWindow.Show();
            window.Close();
        }

        private void SelectJson()
        {
            LoggingManager.LogFormat = "JSON";
            var mainWindow = new MainWindow();
            mainWindow.Show();
            window.Close();
        }
    }
}