using System.IO;

namespace Medior.Utilities
{
    public static class AppFolders
    {
        public static string AppData =>
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medior")).FullName;

        public static string LogsPath =>
            Directory.CreateDirectory(Path.Combine(TempPath, "Logs")).FullName;

        public static string PhotoSorterLogsPath =>
            Directory.CreateDirectory(Path.Combine(TempPath, "PhotoSorter")).FullName;

        public static string RecordingsPath =>
            Directory.CreateDirectory(Path.Combine(TempPath, "Recordings")).FullName;

        public static string TempBin =>
            Directory.CreateDirectory(Path.Combine(TempPath, "bin")).FullName;

        public static string TempPath =>
            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Medior")).FullName;
    }
}
