using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class LanguageSelectionWindow : Window
    {
        public LanguageSelectionWindow()
        {
            InitializeComponent();
            DataContext = new LanguageSelectionViewModel(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}