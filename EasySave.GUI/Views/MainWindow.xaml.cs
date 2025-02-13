using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySave.GUI.ViewModels;
using EasySave.Models;

namespace EasySave.GUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        
        private void BackupItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Vérifie que c'est un double-clic
            if (e.ClickCount == 2)
            {
                if (sender is Border border && border.DataContext is Backup backup)
                {
                    // Bascule la propriété IsSelected
                    backup.IsSelected = !backup.IsSelected;
                }
            }
        }
    }
}