﻿using Microsoft.Extensions.Logging;
using Medior.Core.PhotoSorter.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Medior.Core.Shared.Models;
using Medior.Core.Shared.Utilities;
using Medior.Core.Shared.Services;

namespace Medior.Core.PhotoSorter.Services
{
    public interface IJobWatcher
    {
        Task CancelWatchers();
        Task WatchJobs(string configPath, bool dryRun);
    }

    public class JobWatcher : IJobWatcher
    {
        private static readonly List<FileSystemWatcher> _watchers = new();
        private static readonly SemaphoreSlim _watchersLock = new(1, 1);
        private static readonly ConcurrentDictionary<object, SemaphoreSlim> _jobRunLocks = new();

        private readonly IJobRunner _jobRunner;
        private readonly IReportWriter _reportWriter;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<JobWatcher> _logger;

        public JobWatcher(
            IJobRunner jobRunner, 
            IReportWriter reportWriter,
            IFileSystem fileSystem,
            ILogger<JobWatcher> logger)
        {
            _jobRunner = jobRunner;
            _reportWriter = reportWriter;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public Task CancelWatchers()
        {
            foreach (var watcher in _watchers)
            {
                try
                {
                    watcher.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while disposing of watcher.");
                }
                finally
                {
                    _watchers.Remove(watcher);
                }
            }
            return Task.CompletedTask;
        }

        public async Task WatchJobs(string configPath, bool dryRun)
        {
            try
            {
                await _watchersLock.WaitAsync();

                if (string.IsNullOrWhiteSpace(configPath))
                {
                    throw new ArgumentNullException(nameof(configPath));
                }

                var configString = await _fileSystem.ReadAllTextAsync(configPath);
                var config = JsonSerializer.Deserialize<MediorConfig>(configString);

                if (config is null)
                {
                    throw new SerializationException("Config file could not be deserialized.");
                }

                await CancelWatchers();

                foreach (var job in config.SortJobs)
                {
                    var watcher = new FileSystemWatcher(job.SourceDirectory);

                    foreach (var ext in job.IncludeExtensions)
                    {
                        watcher.Filters.Add($"*.{ext.Replace(".", "")}");
                    }

                    _watchers.Add(watcher);

                    watcher.Created += (sender, ev) =>
                    {
                        _ = RunJob(sender, job, dryRun);
                    };

                    watcher.EnableRaisingEvents = true;
                }
            }
            finally
            {
                _watchersLock.Release();
            }
        }

        private async Task RunJob(object sender, SortJob job, bool dryRun)
        {
            var jobRunLock = _jobRunLocks.GetOrAdd(sender, key =>
            {
                return new SemaphoreSlim(1, 1);
            });

            if (!await jobRunLock.WaitAsync(0))
            {
                return;
            }

            Debouncer.Debounce(TimeSpan.FromSeconds(5), async () =>
            {
                try
                {
                    var report = await _jobRunner.RunJob(job, dryRun, CancellationToken.None);
                    await _reportWriter.WriteReport(report);
                }
                finally
                {
                    jobRunLock.Release();
                }
            });
         
        }
    }
}
