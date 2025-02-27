using System.Diagnostics;
using System.IO;

namespace EasySave.Utils
{
    /// <summary>
    /// Provides a method to check if the business software is running.
    /// </summary>
    public static class BusinessSoftwareChecker
    {
        public static bool IsBusinessSoftwareRunning()
        {
            if (string.IsNullOrWhiteSpace(GeneralSettings.BusinessSoftwareName))
                return false; 

            string searchName = GeneralSettings.BusinessSoftwareName.ToLower();
            // Retrieve all processes and check if any contain the searched string.
            return Process.GetProcesses()
                .Any(p => p.ProcessName.ToLower().Contains(searchName));
        }
    }
}