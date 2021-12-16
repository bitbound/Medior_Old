using Medior.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;

namespace Medior.Services
{
    public class FileLogger : ILogger
    {
        private static readonly ConcurrentQueue<string> _logQueue = new();
        private static readonly ConcurrentStack<string> _scopeStack = new();
        private static readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly ISystemTime _systemTime;
        private readonly IFileSystem _fileSystem;
        private readonly string _categoryName;
        private readonly System.Timers.Timer _sinkTimer = new(5000) { AutoReset = false };

        public FileLogger(ISystemTime systemTime, IFileSystem fileSystem, string categoryName)
        {
            _systemTime = systemTime;
            _fileSystem = fileSystem;
            _categoryName = categoryName;
            _sinkTimer.Elapsed += SinkTimer_Elapsed;
        }

        private string LogPath
        {
            get
            {
                return Path.Combine(AppFolders.LogsPath, $"{_systemTime.Now:yyyy-MM-dd}.log");
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            _scopeStack.Push($"{state}");
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
#if DEBUG
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return true;
#endif
                case LogLevel.Information:
                case LogLevel.Warning:
                case LogLevel.Error:
                case LogLevel.Critical:
                    return true;
                case LogLevel.None:
                    break;
                default:
                    break;
            }
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var scopeStack = _scopeStack.Any() ?
                    new string[] { _scopeStack.First(), _scopeStack.First() } :
                    Array.Empty<string>();

                var message = FormatLogEntry(logLevel, _categoryName, $"{state}", exception, scopeStack);
                _logQueue.Enqueue(message);
                _sinkTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error queueing log entry: {ex.Message}");
            }
        }


        private string FormatLogEntry(LogLevel logLevel, string categoryName, string state, Exception? exception, string[] scopeStack)
        {
            var ex = exception;
            var exMessage = exception?.Message;

            while (ex?.InnerException is not null)
            {
                exMessage += $" | {ex.InnerException.Message}";
                ex = ex.InnerException;
            }

            return $"[{logLevel}]\t" +
                $"{_systemTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t" +
                (
                    scopeStack.Any() ?
                        $"[{string.Join(" - ", scopeStack)} - {categoryName}]\t" :
                        $"[{categoryName}]\t"
                ) +
                $"Message: {state}\t" +
                $"Exception: {exMessage}";
        }

        private async void SinkTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await _writeLock.WaitAsync();

                var lines = new List<string>();

                while (_logQueue.TryDequeue(out var entry))
                {
                    lines.Add(entry);
                }

                var dirPath = Path.GetDirectoryName(LogPath);
                if (string.IsNullOrWhiteSpace(dirPath))
                {
                    return;
                }

                if (!_fileSystem.FileExists(LogPath))
                {
                    _fileSystem.CreateDirectory(dirPath);
                    _fileSystem.CreateFile(LogPath).Close();
                }

                await _fileSystem.AppendAllLinesAsync(LogPath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log entry: {ex.Message}");
            }
            finally
            {
                _writeLock.Release();
            }
        }
        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
                _scopeStack.TryPop(out _);
            }
        }
    }
}
