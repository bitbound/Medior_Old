using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Medior.Core.Services
{
    public class SortBackgroundService : BackgroundService
    {
        private readonly IJobRunner _jobRunner;
        private readonly IJobWatcher _jobWatcher;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IReportWriter _reportWriter;
        private readonly IGlobalState _globalState;
        private readonly ILogger<SortBackgroundService> _logger;

        public SortBackgroundService(
            IJobRunner jobRunner, 
            IJobWatcher jobWatcher,
            IHostApplicationLifetime appLifetime,
            IReportWriter reportWriter,
            IGlobalState globalState,

            ILogger<SortBackgroundService> logger)
        {
            _jobRunner = jobRunner;
            _jobWatcher = jobWatcher;
            _appLifetime = appLifetime;
            _reportWriter = reportWriter;
            _globalState = globalState;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var configPath = _globalState.ConfigPath;

                if (string.IsNullOrWhiteSpace(configPath))
                {
                    _logger.LogInformation("Config path not specified.  Looking for config.json in application directory.");

                    var exeDir = Path.GetDirectoryName(Environment.CommandLine.Split(" ").First());
                    if (string.IsNullOrWhiteSpace(exeDir))
                    {
                        throw new DirectoryNotFoundException("Unable to determine EXE dir.");
                    }

                    configPath = Path.Combine(exeDir, "config.json");

                    if (File.Exists(configPath))
                    {
                        _logger.LogInformation("Found config file: {configPath}.", configPath);
                    }
                    else
                    {
                        _logger.LogWarning("No config file was found at {configPath}.  Exiting.", configPath);
                        _appLifetime.StopApplication();
                        return;
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(_globalState.JobName))
                {
                    _appLifetime.StopApplication();
                    var report = await _jobRunner.RunJob(configPath, _globalState.JobName, _globalState.DryRun);
                    await _reportWriter.WriteReport(report);
                    return;
                }

                var reports = await _jobRunner.RunJobs(configPath, _globalState.DryRun);
                await _reportWriter.WriteReports(reports);
                
                if (_globalState.Once)
                {
                    _appLifetime.StopApplication();
                    return;
                }

                await _jobWatcher.WatchJobs(configPath, _globalState.DryRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while starting background service.");
            }
        }
    }
}
