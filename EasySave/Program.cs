using System;
using System.Collections.Generic;
using EasySave.Controllers;

namespace EasySave
{
    /// <summary>
    /// Entry point for the EasySave application.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                // If command-line arguments are provided, parse them to execute backups automatically.
                List<int> indices = ParseIndices(args[0]);
                BackupController controller = new BackupController();
                foreach (int i in indices)
                {
                    // Assume indices are 1-indexed; subtract 1 for 0-indexed.
                    controller.ExecuteBackup(i - 1);
                }
            }
            else
            {
                // Otherwise, launch the interactive console interface.
                var view = new Views.ConsoleView();
                view.Start();
            }
        }

        /// <summary>
        /// Parses an argument string to obtain a list of indices.
        /// Supports formats like "1-3" (range) and "1;3" (semicolon-separated).
        /// </summary>
        static List<int> ParseIndices(string arg)
        {
            List<int> indices = new List<int>();
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
                if (int.TryParse(arg, out int index))
                {
                    indices.Add(index);
                }
            }
            return indices;
        }
    }
}
