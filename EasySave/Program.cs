using System;
using System.Collections.Generic;
using EasySave;
using EasySave.Controllers;
using EasySave.Logging;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                // Si des arguments sont passés, on les interprète pour exécuter automatiquement des backups.
                List<int> indices = ParseIndices(args[0]);
                BackupController controller = new BackupController();
                foreach (int i in indices)
                {
                    // On suppose que l'indice dans le fichier est 1-indexé, on décrémente donc de 1.
                    controller.ExecuteBackup(i - 1);
                }
            }
            else
            {
                // Sinon, on démarre l'interface interactive
                var view = new Views.ConsoleView();
                view.Start();
            }
        }

        /// <summary>
        /// Parse une chaîne d'arguments pour obtenir une liste d'indices.
        /// Supporte des formats comme "1-3" (plage) et "1;3" (séparés par point-virgule).
        /// </summary>
        static List<int> ParseIndices(string arg)
        {
            List<int> indices = new List<int>();
            // On peut supporter les deux séparateurs : '-' pour plage, ';' pour liste
            if (arg.Contains("-"))
            {
                var parts = arg.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int start) &&
                    int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        indices.Add(i);
                    }
                }
            }
            else if (arg.Contains(";"))
            {
                var parts = arg.Split(';');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int index))
                    {
                        indices.Add(index);
                    }
                }
            }
            else
            {
                // Si l'argument est un seul indice
                if (int.TryParse(arg, out int index))
                {
                    indices.Add(index);
                }
            }
            return indices;
        }
    }
}
