using System;

namespace EasySave.Utils
{
    /// <summary>
    /// Static class to store general application settings.
    /// The user can define the business software name here.
    /// For example, for demonstrations, "calc" can be used for the calculator.
    /// </summary>
    public static class GeneralSettings
    {
        // Default is empty (no business software defined)
// The user can modify it via the settings interface.
        public static string BusinessSoftwareName { get; set; } = "";
        
        public static string AllowedEncryptionFileTypes { get; set; } = ".txt,.docx,.pdf";
        
        public static long MaxLargeFileSize { get; set; } = 300 * 1024;
        public static string PriorityExtensions { get; set; } = ".exe,.pdf"; 


    }
}