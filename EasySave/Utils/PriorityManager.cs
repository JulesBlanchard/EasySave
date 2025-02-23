using System;

namespace EasySave.Utils
{
    public static class PriorityManager
    {
        private static int _globalPendingPriorityFiles = 0;
        private static readonly object _lock = new object();

        public static void AddPriorityFiles(int count)
        {
            lock (_lock)
            {
                _globalPendingPriorityFiles += count;
            }
        }

        public static void DecrementPriorityFiles()
        {
            lock (_lock)
            {
                if (_globalPendingPriorityFiles > 0)
                    _globalPendingPriorityFiles--;
            }
        }

        public static int GetPendingCount()
        {
            lock (_lock)
            {
                return _globalPendingPriorityFiles;
            }
        }
    }
}