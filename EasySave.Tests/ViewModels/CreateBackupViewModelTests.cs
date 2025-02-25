using System.ComponentModel;
using Xunit;
using EasySave.GUI.ViewModels;
using System.Windows;

namespace EasySave.Tests.ViewModels
{
    public class CreateBackupViewModelTests
    {
        [Fact]
        public void SettingBackupName_ShouldRaisePropertyChanged()
        {
            // Arrange : on instancie le ViewModel en simulant une fenêtre (ici, une Window dummy)
            var window = new Window();
            var vm = new CreateBackupViewModel(window);
            bool propertyChangedRaised = false;
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(vm.BackupName))
                    propertyChangedRaised = true;
            };

            // Act
            vm.BackupName = "TestBackup";

            // Assert
            Assert.True(propertyChangedRaised, "La modification de la propriété BackupName doit lever PropertyChanged.");
        }

        [Fact]
        public void CreateBackupCommand_ShouldNotCreateBackup_WhenBackupNameIsEmpty()
        {
            // Arrange
            var window = new Window();
            var vm = new CreateBackupViewModel(window);
            vm.BackupName = ""; // nom vide
            vm.SourcePath = "C:\\Temp"; // chemin supposé existant
            vm.TargetPath = "C:\\TempTarget";

            // Pour ce test, on simule l'action de fermeture de la fenêtre via CloseAction
            bool windowClosed = false;
            vm.CloseAction = () => windowClosed = true;

            // Act
            vm.CreateBackupCommand.Execute(null);

            // Assert : la fenêtre ne doit pas se fermer car le nom est vide
            Assert.False(windowClosed, "La commande de création ne doit pas fermer la fenêtre si le nom est vide.");
        }
    }
}