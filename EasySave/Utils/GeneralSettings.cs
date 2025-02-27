using System;

namespace EasySave.Utils
{
    /// <summary>
    /// Classe statique pour stocker les paramètres généraux de l'application.
    /// L'utilisateur pourra définir ici le nom du logiciel métier.
    /// Par exemple, pour les démonstrations, "calc" pourra être utilisé pour la calculatrice.
    /// </summary>
    public static class GeneralSettings
    {
        // Par défaut, vide (aucun logiciel métier défini)
        // L'utilisateur pourra le modifier via l'interface de paramètres.
        public static string BusinessSoftwareName { get; set; } = "";
        
        public static string AllowedEncryptionFileTypes { get; set; } = ".txt,.docx,.pdf";
        
        public static long MaxLargeFileSize { get; set; } = 300 * 1024;
        public static string PriorityExtensions { get; set; } = "exe,.pdf"; 


    }
}