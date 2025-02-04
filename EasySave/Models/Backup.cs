using System;
using System.Collections.Generic;
using System.IO;

namespace EasySave.Models
{
    public class Backup
    {
        public string Name { get; private set; }
        public string SourcePath { get; private set; }
        public string TargetPath { get; private set; }
        
        public IBackupStrategy Strategy { get; set; }

        public Backup(string name, string source, string target)
        {
            Name = name;
            SourcePath = source;
            TargetPath = target;
        }

        /// <summary>
        /// On reçoit un logger, on passe tout à Strategy.Execute(...)
        /// </summary>
        public void Execute(IBackupLogger logger)
        {
            if (Strategy != null)
            {
                Strategy.Execute(this, logger);
            }
            else
            {
                Console.WriteLine($"[Backup] No strategy set for '{Name}'. Nothing to do.");
            }
        }

        public List<FileInfo> GetFileList()
        {
            var result = new List<FileInfo>();
            if (Directory.Exists(SourcePath))
            {
                var allFiles = Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories);
                foreach (var f in allFiles)
                {
                    result.Add(new FileInfo(f));
                }
            }
            return result;
        }
    }
}