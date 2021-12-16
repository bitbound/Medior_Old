using Microsoft.Extensions.Logging;

namespace Medior.Services
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly ISystemTime _systemTime;
        private readonly IFileSystem _fileSystem;

        public FileLoggerProvider(ISystemTime systemTime, IFileSystem fileSystem)
        {
            _systemTime = systemTime;
            _fileSystem = fileSystem;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_systemTime, _fileSystem, categoryName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
