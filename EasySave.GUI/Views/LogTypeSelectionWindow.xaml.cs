using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class LogTypeSelectionWindow : Window
    {
        public LogTypeSelectionWindow()
        {
            InitializeComponent();
            DataContext = new LogTypeSelectionViewModel(this);
        }
    }
}