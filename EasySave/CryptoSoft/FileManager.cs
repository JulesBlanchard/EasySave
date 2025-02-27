using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace CryptoSoft
{
    /// <summary>
    /// File manager class (Mono-instance version)
    /// Cette version utilise un mutex nommé pour s'assurer qu'une seule instance de CryptoSoft
    /// est active sur la machine.
    /// </summary>
    public class FileManager
    {
        // Mutex global pour garantir une seule instance à travers tous les processus.
        private static readonly Mutex cryptoMutex = new Mutex(false, "Global\\CryptoSoft_Mutex");

        private string FilePath { get; }
        private string Key { get; }

        public FileManager(string path, string key)
        {
            FilePath = path;
            Key = key;
        }

        /// <summary>
        /// Vérifie que le fichier existe.
        /// </summary>
        private bool CheckFile()
        {
            if (File.Exists(FilePath))
                return true;

            Console.WriteLine("File not found.");
            Thread.Sleep(1000);
            return false;
        }

        /// <summary>
        /// Chiffre le fichier en utilisant un algorithme XOR tout en s'assurant que seule une instance
        /// de CryptoSoft s'exécute à la fois.
        /// </summary>
        public int TransformFile()
        {
            if (!CheckFile())
                return -1;

            bool hasHandle = false;
            try
            {
                // Tente d'acquérir le mutex avec un délai d'attente (par exemple 1000 ms).
                hasHandle = cryptoMutex.WaitOne(1000, false);
                if (!hasHandle)
                {
                    Console.WriteLine("CryptoSoft est déjà en cours d'exécution. Veuillez patienter.");
                    return -1; // On peut aussi lever une exception si nécessaire.
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                var fileBytes = File.ReadAllBytes(FilePath);
                var keyBytes = ConvertToByte(Key);
                fileBytes = XorMethod(fileBytes, keyBytes);
                File.WriteAllBytes(FilePath, fileBytes);
                stopwatch.Stop();
                return (int)stopwatch.ElapsedMilliseconds;
            }
            finally
            {
                // Libère le mutex si nous l'avons acquis.
                if (hasHandle)
                    cryptoMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Convertit une chaîne de caractères en tableau d'octets.
        /// </summary>
        private static byte[] ConvertToByte(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Applique une opération XOR sur les octets du fichier avec les octets de la clé.
        /// </summary>
        private static byte[] XorMethod(IReadOnlyList<byte> fileBytes, IReadOnlyList<byte> keyBytes)
        {
            var result = new byte[fileBytes.Count];
            for (var i = 0; i < fileBytes.Count; i++)
            {
                result[i] = (byte)(fileBytes[i] ^ keyBytes[i % keyBytes.Count]);
            }
            return result;
        }
    }
}
