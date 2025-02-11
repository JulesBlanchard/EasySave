using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace EasySave.Models
{
    /// <summary>
    /// Static class that manages and updates the state of backups.
    /// It writes the state file in JSON or XML format based on LoggingManager.LogFormat.
    /// </summary>
    public static class StateManager
    {
        // File paths for JSON and XML state files.
        private static readonly string stateFilePathJson = Path.Combine(AppContext.BaseDirectory, "state.json");
        private static readonly string stateFilePathXml = Path.Combine(AppContext.BaseDirectory, "state.xml");

        // Dictionary to store backup states, indexed by backup name.
        private static Dictionary<string, BackupState> states = new Dictionary<string, BackupState>();

        /// <summary>
        /// Updates or adds the state of a backup and writes the state file.
        /// </summary>
        /// <param name="state">The backup state to update</param>
        public static void UpdateState(BackupState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.Name))
                return;

            states[state.Name] = state;
            WriteStateFile();
        }

        /// <summary>
        /// Writes the state file using either JSON or XML format based on LoggingManager.LogFormat.
        /// </summary>
        private static void WriteStateFile()
        {
            if (EasySave.Logging.LoggingManager.LogFormat.ToUpper() == "XML")
            {
                WriteStateFileXml();
            }
            else
            {
                WriteStateFileJson();
            }
        }

        /// <summary>
        /// Serializes and writes the backup states to a JSON file.
        /// </summary>
        private static void WriteStateFileJson()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                // Convert enums to strings.
                options.Converters.Add(new JsonStringEnumConverter());
                string json = JsonSerializer.Serialize(states.Values.ToList(), options);
                File.WriteAllText(stateFilePathJson, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[StateManager] Error writing state file (JSON): " + ex.Message);
            }
        }

        /// <summary>
        /// Serializes and writes the backup states to an XML file.
        /// </summary>
        private static void WriteStateFileXml()
        {
            try
            {
                var root = new XElement("BackupStates",
                    from state in states.Values
                    select new XElement("BackupState",
                        new XElement("Name", state.Name),
                        new XElement("SourceFilePath", state.SourceFilePath),
                        new XElement("TargetFilePath", state.TargetFilePath),
                        new XElement("Status", state.Status.ToString()),
                        new XElement("TotalFilesToCopy", state.TotalFilesToCopy),
                        new XElement("TotalFilesSize", state.TotalFilesSize),
                        new XElement("TotalFilesSizeUnit", state.TotalFilesSizeUnit),
                        new XElement("NbFilesLeftToDo", state.NbFilesLeftToDo),
                        new XElement("Progression", state.Progression),
                        new XElement("LastActionTimestamp", state.LastActionTimestamp)
                    )
                );
                XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
                doc.Save(stateFilePathXml);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[StateManager] Error writing state file (XML): " + ex.Message);
            }
        }
    }
}
