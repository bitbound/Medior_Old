using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Utilities
{
    public static class AppFolders
    {
        public static string LogsPath => Path.Combine(TempPath, "Logs");
        public static string PhotoSorterLogsPath => Path.Combine(TempPath, "PhotoSorter");
        public static string RecordingsPath => Path.Combine(TempPath, "Recordings");
        public static string TempBin => Path.Combine(TempPath, "bin");
        public static string TempPath => Path.Combine(Path.GetTempPath(), "Medior");
    }
}
