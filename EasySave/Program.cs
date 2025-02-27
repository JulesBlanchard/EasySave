using System;
using System.Diagnostics;
using System.Collections.Generic;
using EasySave.Controllers;
using EasySave.Logging;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            // Prompt for log format selection
            Console.WriteLine("Choose log format:");
            Console.WriteLine("1. JSON");
            Console.WriteLine("2. XML");
            Console.Write("Your choice: ");
            string formatChoice = Console.ReadLine().Trim();
            if (formatChoice == "2")
            {
                LoggingManager.LogFormat = "XML";
            }

            // Then, launch the interactive interface (or process args as before)
            if (args.Length > 0)
            {
                // Command-line execution (unchanged)
                List<int> indices = ParseIndices(args[0]);
                BackupController controller = new BackupController();
                foreach (int i in indices)
                {
                    controller.ExecuteBackup(i - 1);
                }
            }

        }

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
