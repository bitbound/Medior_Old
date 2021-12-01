using Microsoft.Extensions.Logging;

namespace Medior.Services
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _services;

        public FileLoggerProvider(IServiceProvider services)
        {
            _services = services;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_services, categoryName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
