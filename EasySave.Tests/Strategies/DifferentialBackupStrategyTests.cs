using System;
using System.IO;
using Xunit;
using EasySave.Models;
using EasySave.Logging;
using EasySave.Utils;

namespace EasySave.Tests.Strategies
{
    public class DifferentialBackupStrategyTests
    {
        [Fact]
        public void Execute_ShouldCopyNewOrUpdatedFiles()
        {
            // Arrange : créer des dossiers temporaires pour source et target
            string tempSource = Path.Combine(Path.GetTempPath(), "TestSource_" + Guid.NewGuid());
            string tempTarget = Path.Combine(Path.GetTempPath(), "TestTarget_" + Guid.NewGuid());
            Directory.CreateDirectory(tempSource);
            Directory.CreateDirectory(tempTarget);

            // Créer un fichier dans la source
            string sourceFile = Path.Combine(tempSource, "test.txt");
            File.WriteAllText(sourceFile, "Contenu original");

            // Créer un backup avec la stratégie différentielle
            Backup backup = new Backup("BackupTest", tempSource, tempTarget, "diff")
            {
                ShouldEncrypt = false // désactiver le cryptage pour ce test
            };

            // Utiliser un logger (vous pouvez également utiliser FakeLogger ici)
            IBackupLogger logger = LoggingManager.GetLogger("TestLogs");
            DifferentialBackupStrategy strategy = new DifferentialBackupStrategy();

            // Act : exécuter la sauvegarde
            strategy.Execute(backup, logger);

            // Assert : vérifier que le fichier a été copié dans le dossier cible
            string targetFile = Path.Combine(tempTarget, "test.txt");
            Assert.True(File.Exists(targetFile), "Le fichier source doit être copié dans le dossier cible.");

            // Nettoyer les dossiers temporaires
            Directory.Delete(tempSource, true);
            Directory.Delete(tempTarget, true);
        }
    }
}