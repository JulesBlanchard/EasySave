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
    /// Classe statique qui gère et met à jour l'état des sauvegardes.
    /// Elle écrit le fichier d'état au format JSON ou XML selon LoggingManager.LogFormat.
    /// </summary>
    public static class StateManager
    {
        // Chemins des fichiers d'état pour JSON et XML.
        private static readonly string stateFilePathJson = Path.Combine(AppContext.BaseDirectory, "state.json");
        private static readonly string stateFilePathXml = Path.Combine(AppContext.BaseDirectory, "state.xml");

        // Dictionnaire pour stocker les états des sauvegardes, indexé par le nom de la sauvegarde.
        private static Dictionary<string, BackupState> states = new Dictionary<string, BackupState>();

        // Verrou statique pour synchroniser l'accès aux fichiers d'état.
        private static readonly object _stateFileLock = new object();

        /// <summary>
        /// Met à jour ou ajoute l'état d'une sauvegarde et écrit le fichier d'état.
        /// </summary>
        public static void UpdateState(BackupState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.Name))
                return;

            states[state.Name] = state;
            WriteStateFile();
        }

        /// <summary>
        /// Écrit le fichier d'état en JSON ou XML selon LoggingManager.LogFormat.
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
        /// Sérialise et écrit les états des sauvegardes dans un fichier JSON.
        /// </summary>
        private static void WriteStateFileJson()
        {
            lock (_stateFileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    // Convertir les enums en chaînes
                    options.Converters.Add(new JsonStringEnumConverter());
                    string json = JsonSerializer.Serialize(states.Values.ToList(), options);
                    File.WriteAllText(stateFilePathJson, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[StateManager] Error writing state file (JSON): " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Sérialise et écrit les états des sauvegardes dans un fichier XML.
        /// </summary>
        private static void WriteStateFileXml()
        {
            lock (_stateFileLock)
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
}
