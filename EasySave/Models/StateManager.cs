using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySave.Models
{
    public static class StateManager
    {
        // Chemin complet du fichier state.json dans le répertoire d'exécution
        private static readonly string stateFilePath = Path.Combine(AppContext.BaseDirectory, "state.json");

        // Dictionnaire des états, indexé par le nom du backup
        private static Dictionary<string, BackupState> states = new Dictionary<string, BackupState>();

        /// <summary>
        /// Met à jour ou ajoute l'état d'un backup et réécrit le fichier state.json.
        /// </summary>
        public static void UpdateState(BackupState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.Name))
                return;

            states[state.Name] = state;
            WriteStateFile();
        }

        /// <summary>
        /// Réécrit le fichier state.json avec la liste des états.
        /// </summary>
        private static void WriteStateFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                // Sérialise la liste des états en un tableau JSON
                string json = JsonSerializer.Serialize(states.Values.ToList(), options);
                File.WriteAllText(stateFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[StateManager] Erreur lors de l'écriture du fichier d'état : " + ex.Message);
            }
        }
    }
}