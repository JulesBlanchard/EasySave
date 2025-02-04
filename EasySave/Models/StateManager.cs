using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    /// <summary>
    /// Static class that manages and updates the state of backups.
    /// </summary>
    public static class StateManager
    {
        // Full path to the state.json file in the execution directory.
        private static readonly string stateFilePath = Path.Combine(AppContext.BaseDirectory, "state.json");

        // Dictionary of states, indexed by backup name.
        private static Dictionary<string, BackupState> states = new Dictionary<string, BackupState>();

        /// <summary>
        /// Updates or adds the state of a backup and writes the state.json file.
        /// </summary>
        public static void UpdateState(BackupState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.Name))
                return;

            states[state.Name] = state;
            WriteStateFile();
        }

        /// <summary>
        /// Writes the state.json file with the list of states.
        /// </summary>
        private static void WriteStateFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                // Add the converter to serialize enums as strings.
                options.Converters.Add(new JsonStringEnumConverter());
                
                // Serialize the list of states as a JSON array.
                string json = JsonSerializer.Serialize(states.Values.ToList(), options);
                File.WriteAllText(stateFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[StateManager] Error writing state file: " + ex.Message);
            }
        }
    }
}