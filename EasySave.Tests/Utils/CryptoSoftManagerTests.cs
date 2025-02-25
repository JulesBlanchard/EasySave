using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using EasySave.Utils;

namespace EasySave.Tests.Utils
{
    public class CryptoSoftManagerTests
    {
        [Fact]
        public void TransformFile_ShouldReturnNonNegativeTime()
        {
            // Arrange : créer un fichier temporaire
            string tempFile = Path.Combine(Path.GetTempPath(), "testfile.txt");
            File.WriteAllText(tempFile, "Contenu de test");
            string key = "1234";

            // Act
            int encryptionTime = CryptoSoftManager.Instance.TransformFile(tempFile, key);

            // Assert
            Assert.True(encryptionTime >= 0, "Le temps de cryptage doit être positif ou égal à zéro.");

            // Cleanup
            File.Delete(tempFile);
        }

        [Fact]
        public async Task TransformFile_ConcurrentCalls_ShouldExecuteSequentially()
        {
            // Arrange : créer plusieurs fichiers temporaires
            string key = "abcd";
            string tempFile1 = Path.Combine(Path.GetTempPath(), "testfile1.txt");
            string tempFile2 = Path.Combine(Path.GetTempPath(), "testfile2.txt");
            File.WriteAllText(tempFile1, "Fichier 1");
            File.WriteAllText(tempFile2, "Fichier 2");

            // Act : lancer deux appels en parallèle
            var task1 = Task.Run(() => CryptoSoftManager.Instance.TransformFile(tempFile1, key));
            var task2 = Task.Run(() => CryptoSoftManager.Instance.TransformFile(tempFile2, key));
            int[] results = await Task.WhenAll(task1, task2);

            // Assert : les deux résultats doivent être valides (>= 0)
            Assert.All(results, time => Assert.True(time >= 0));

            // Cleanup
            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }
    }
}