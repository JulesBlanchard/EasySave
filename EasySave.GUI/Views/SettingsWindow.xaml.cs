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
            // On assigne l'action qui ferme la fenêtre
            vm.CloseAction = new Action(() => this.Close());
            DataContext = vm;
        }
    }
}