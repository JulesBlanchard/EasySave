using System;
using System.Windows;
using System.Windows.Input;
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

        private void SetLanguage(string lang)
        {
            // Choose the dictionary to load based on the language parameter
            string uri = lang == "fr" ? "Ressources/Strings.fr.xaml" : "Ressources/Strings.en.xaml";
            ResourceDictionary dict = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };

            // Explicit use of System.Windows.Application to resolve ambiguity
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void SelectFrench()
        {
            SetLanguage("fr");
            var logTypeWindow = new LogTypeSelectionWindow();
            logTypeWindow.Show();
            window.Close();
        }

        private void SelectEnglish()
        {
            SetLanguage("en");
            var logTypeWindow = new LogTypeSelectionWindow();
            logTypeWindow.Show();
            window.Close();
        }
    }
}