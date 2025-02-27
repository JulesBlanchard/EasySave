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
        /// Resets the flag to allow displaying a new popup 
        /// upon a new detection.
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