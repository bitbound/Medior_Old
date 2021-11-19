using System.IO;

namespace Medior.Services
{
    public interface IEnvironmentService
    {
        bool IsDebug { get; }
        string LogsPath { get; }
        string PhotoSorterLogsPath { get; }
        string TempBin { get; }
        string TempPath { get; }
    }

    public class EnvironmentService : IEnvironmentService
    {
        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public string LogsPath => Path.Combine(TempPath, "Logs");
        public string PhotoSorterLogsPath => Path.Combine(TempPath, "PhotoSorter");
        public string TempBin => Path.Combine(TempPath, "bin");
        public string TempPath => Path.Combine(Path.GetTempPath(), "Medior");
    }
}
