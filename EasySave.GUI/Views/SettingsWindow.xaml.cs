using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}