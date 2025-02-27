namespace EasySave.Logging
{
    /// <summary>
    /// Provides a factory method to obtain a logger (JSON or XML) based on configuration.
    /// </summary>
    public static class LoggingManager
    {
        // Static property to choose the log format; default is "JSON".
        public static string LogFormat { get; set; } = "JSON";

        /// <summary>
        /// Returns an IBackupLogger based on the selected log format.
        /// </summary>
        /// <param name="logDirectory">The directory where log files are stored.</param>
        public static IBackupLogger GetLogger(string logDirectory)
        {
            if (LogFormat.ToUpper() == "XML")
            {
                return new XmlBackupLogger(logDirectory);
            }
            else
            {
                return new JsonBackupLogger(logDirectory);
            }
        }
    }
}