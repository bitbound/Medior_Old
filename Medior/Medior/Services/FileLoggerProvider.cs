using Microsoft.Extensions.Logging;

namespace Medior.Services
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _services;
        private readonly string _appName;

        public FileLoggerProvider(IServiceProvider services, string appName)
        {
            _services = services;
            _appName = appName;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_services, _appName, categoryName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
