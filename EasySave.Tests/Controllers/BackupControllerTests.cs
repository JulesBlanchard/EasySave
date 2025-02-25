using System.Linq;
using Xunit;
using EasySave.Controllers;
using EasySave.Models;

namespace EasySave.Tests.Controllers
{
    public class BackupControllerTests
    {
        [Fact]
        public void CreateBackup_ShouldAddBackupToList()
        {
            // Arrange
            BackupController controller = BackupController.Instance;
            int initialCount = controller.GetBackups().Count;
            string backupName = "TestBackup";
            string source = "C:\\SourceFolder";
            string target = "C:\\TargetFolder";
            string strategyType = "full";
            bool shouldEncrypt = false;
            string encryptionKey = string.Empty;

            // Act : créer une sauvegarde
            controller.CreateBackup(backupName, source, target, strategyType, shouldEncrypt, encryptionKey);
            var backups = controller.GetBackups();

            // Assert : le nombre de sauvegardes a augmenté et le backup ajouté possède le bon nom
            Assert.Equal(initialCount + 1, backups.Count);
            Assert.Contains(backups, b => b.Name == backupName);
        }
    }
}