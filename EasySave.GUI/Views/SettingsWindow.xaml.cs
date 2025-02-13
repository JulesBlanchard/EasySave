using System;
using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var vm = new SettingsViewModel();
            // Fermer la fenêtre après sauvegarde
            vm.CloseAction = new Action(() => this.Close());
            DataContext = vm;
        }
    }
}