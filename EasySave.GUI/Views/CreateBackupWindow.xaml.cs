using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class CreateBackupWindow : Window
    {
        public CreateBackupWindow()
        {
            InitializeComponent();
            DataContext = new CreateBackupViewModel(this);
        }
    }
}