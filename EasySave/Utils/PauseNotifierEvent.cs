using System;
using System.Threading;

namespace EasySave.Utils
{
    public static class PauseNotifierEvent
    {
        private static bool popupDisplayed = false;
        private static readonly object lockObj = new object();

        public static event Action PauseRequested;

        public static void RequestPause()
        {
            lock (lockObj)
            {
                if (!popupDisplayed)
                {
                    popupDisplayed = true;
                    PauseRequested?.Invoke();
                }
            }
        }

        /// <summary>
        /// Réinitialise le flag pour permettre l'affichage d'une nouvelle popup 
        /// lors d'une nouvelle détection.
        /// </summary>
        public static void Reset()
        {
            lock (lockObj)
            {
                popupDisplayed = false;
            }
        }
    }
}