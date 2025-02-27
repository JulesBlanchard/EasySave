using System.Windows;
using EasySave.GUI.ViewModels;
using EasySave.Models;

namespace EasySave.GUI.Views
{
    public partial class EditBackupWindow : Window
    {
        public EditBackupWindow(Backup backup)
        {
            InitializeComponent();
            DataContext = new EditBackupViewModel(backup, this);
        }
    }
}