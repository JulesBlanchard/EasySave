using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Set DataContext to MainViewModel instance.
            DataContext = new MainViewModel();
        }
    }
}