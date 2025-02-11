using System.Diagnostics;
using System.IO;

namespace EasySave.Utils
{
    /// <summary>
    /// Fournit une méthode pour vérifier si le logiciel métier est en cours d'exécution.
    /// </summary>
    public static class BusinessSoftwareChecker
    {
        public static bool IsBusinessSoftwareRunning()
        {
            if (string.IsNullOrWhiteSpace(GeneralSettings.BusinessSoftwareName))
                return false; // Aucun logiciel métier défini.

            string searchName = GeneralSettings.BusinessSoftwareName.ToLower();
            // Récupérer tous les processus et vérifier si l'un d'eux contient la chaîne recherchée.
            return Process.GetProcesses()
                .Any(p => p.ProcessName.ToLower().Contains(searchName));
        }
    }
}