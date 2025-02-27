using System.Collections.Generic;

namespace EasySave.Utils
{
    /// <summary>
    /// Manages the currently selected language messages.
    /// </summary>
    public static class LocalizationManager
    {
        // The current messages dictionary; default is French.
        public static Dictionary<string, string> CurrentMessages { get; set; } = Messages.French;
    }
}