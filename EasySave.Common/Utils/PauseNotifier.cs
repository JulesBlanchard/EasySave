using System.Windows;
using System.Windows.Threading;
using EasySave.Utils;

namespace EasySave.Common.Utils
{
    public static class PauseNotifier
    {
        // Flag pour s'assurer que la popup n'est affichée qu'une seule fois.
        private static volatile bool popupShown = false;
     
        /// <summary>
        /// Vérifie si le logiciel métier est lancé et affiche une popup (uniquement une fois)
        /// pour indiquer que les sauvegardes sont en pause.
        /// </summary>
        public static void CheckAndShowPauseNotification()
        {
            if (BusinessSoftwareChecker.IsBusinessSoftwareRunning())
            {
                if (!popupShown)
                {
                    popupShown = true;
                    // Exécution sur le thread UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Les sauvegardes sont en pause car le logiciel métier est en cours d'exécution.\nElles reprendront dès que le logiciel sera fermé.",
                            "Sauvegardes en pause",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    });
                }
            }
            else
            {
                // Réinitialiser le flag une fois le logiciel fermé
                popupShown = false;
            }
        }
    }
}