using System.Windows.Input;
using EasySave.Utils;
using EasySave.GUI.Views;

namespace EasySave.GUI.ViewModels
{
    public class LanguageSelectionViewModel
    {
        private readonly LanguageSelectionWindow window;
        public ICommand SelectFrenchCommand { get; }
        public ICommand SelectEnglishCommand { get; }

        public LanguageSelectionViewModel(LanguageSelectionWindow window)
        {
            this.window = window;
            SelectFrenchCommand = new RelayCommand(SelectFrench);
            SelectEnglishCommand = new RelayCommand(SelectEnglish);
        }

        private void SelectFrench()
        {
            LocalizationManager.CurrentMessages = Messages.French;
            var logTypeWindow = new LogTypeSelectionWindow();
            logTypeWindow.Show();
            window.Close();
        }

        private void SelectEnglish()
        {
            LocalizationManager.CurrentMessages = Messages.English;
            var logTypeWindow = new LogTypeSelectionWindow();
            logTypeWindow.Show();
            window.Close();
        }
    }
}