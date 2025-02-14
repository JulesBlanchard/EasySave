using System;
using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class EncryptFileWindow : Window
    {
        public EncryptFileWindow()
        {
            InitializeComponent();
            var vm = new EncryptFileViewModel();
            vm.CloseAction = new Action(() => this.Close());
            DataContext = vm;
        }
    }
}