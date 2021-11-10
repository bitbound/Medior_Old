using Medior.Core.Shared.BaseTypes;
using Medior.Core.PhotoSorter.Models;
using Medior.Core.PhotoSorter.Services;
using System;
using System.Linq;
using Medior.Core.Shared.Extensions;
using Microsoft.Extensions.Logging;
using Medior.Services;
using Medior.Core.PhotoSorter.Enums;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using System.Threading;
using Microsoft.UI.Dispatching;

namespace Medior.ViewModels
{
    public class PhotoSorterViewModel : ViewModelBase
    {
        private readonly IAppSettings _appSettings;
        private readonly IJobRunner _jobRunner;
        private readonly ILogger<PhotoSorterViewModel> _logger;
        private readonly IPathTransformer _pathTransformer;
        private readonly IReportWriter _reportWriter;
        private readonly IDispatcherService _dispatcherService;
        private string? _currentJobRunnerFile;
        private bool _isDryRun;
        private bool _isJobRunning;
        private SortJob? _selectedJob;
        private int _jobRunnerProgress;

        public PhotoSorterViewModel(
            IAppSettings appSettings,
            IPathTransformer pathTransformer,
            IJobRunner jobRunner,
            IReportWriter reportWriter,
            IDispatcherService dispatcherService,
            ILogger<PhotoSorterViewModel> logger)
        {
            _appSettings = appSettings;
            _pathTransformer = pathTransformer;
            _jobRunner = jobRunner;
            _reportWriter = reportWriter;
            _dispatcherService = dispatcherService;
            _logger = logger;
            
            LoadSortJobs();

            _jobRunner.ProgressChanged += JobRunner_ProgressChanged;
            _jobRunner.CurrentTaskChanged += JobRunner_CurrentTaskChanged;
        }

        public bool IsDryRun
        {
            get => _isDryRun;
            set => SetProperty(ref _isDryRun, value);
        }

        public bool IsJobRunning
        {
            get => _isJobRunning;
            set => SetProperty(ref _isJobRunning, value);
        }

        public SortJob? SelectedJob
        {
            get => _selectedJob;
            set
            {
                SetProperty(ref _selectedJob, value);
                InvokePropertyChanged(nameof(GetIncludeExtensions));
                InvokePropertyChanged(nameof(GetExcludeExtensions));
            }
        }

        public ObservableCollectionEx<SortJob> SortJobs { get; } = new();

        public void CreateNewSortJob(string sortJobName)
        {
            var sortJob = new SortJob()
            {
                Name = sortJobName
            };

            SortJobs.Add(sortJob);
            SaveAppSettings();

            LoadSortJobs();
            SelectedJob = SortJobs.FirstOrDefault(x => x.Id == sortJob.Id);
        }

        public void DeleteSortJob()
        {
            if (SelectedJob is not null)
            {
                SortJobs.Remove(SelectedJob);
                SaveAppSettings();
                LoadSortJobs();
                SelectedJob = SortJobs.FirstOrDefault();
            }
        }

        public string GetDestinationTransform()
        {
            if (string.IsNullOrWhiteSpace(SelectedJob?.DestinationFile))
            {
                return string.Empty;
            }

            return _pathTransformer.TransformPath(
                "Example.ext",
                SelectedJob.DestinationFile,
                DateTime.Now,
                "Example Camera");
        }

        public string GetExcludeExtensions()
        {
            if (SelectedJob?.ExcludeExtensions is null)
            {
                return string.Empty;
            }
            return string.Join(", ", SelectedJob.ExcludeExtensions);
        }

        public string GetIncludeExtensions()
        {
            if (SelectedJob?.IncludeExtensions is null)
            {
                return string.Empty;
            }
            return string.Join(", ", SelectedJob.IncludeExtensions);
        }

        public OverwriteAction[] GetOverwriteActions()
        {
            return Enum.GetValues<OverwriteAction>();
        }

        public SortOperation[] GetSortOperations()
        {
            return Enum.GetValues<SortOperation>();
        }

        public void RenameSortJob(string newName)
        {
            var modifiedJob = SortJobs.FirstOrDefault(x => x.Id == SelectedJob?.Id);
            if (modifiedJob is null)
            {
                return;
            }

            modifiedJob.Name = newName;
            SaveAppSettings();
            LoadSortJobs();
            SelectedJob = SortJobs.FirstOrDefault(x => x.Id == modifiedJob.Id);
        }

        public void SaveJob()
        {
            if (_selectedJob is null)
            {
                return;
            }

            SaveAppSettings();
        }

        public void SetExcludeExtensions(string extensions)
        {
            if (SelectedJob is null)
            {
                return;
            }

            SelectedJob.ExcludeExtensions = extensions.Split(",", StringSplitOptions.TrimEntries);
            SaveAppSettings();
        }

        public void SetIncludeExtensions(string extensions)
        {
            if (SelectedJob is null)
            {
                return;
            }

            SelectedJob.IncludeExtensions = extensions.Split(",", StringSplitOptions.TrimEntries);
            SaveAppSettings();
        }

        public async Task<JobReport> StartJob(CancellationToken cancellationToken)
        {
            Guard.IsNotNull(SelectedJob, nameof(SelectedJob));
            var report = await _jobRunner.RunJob(SelectedJob, IsDryRun, cancellationToken);
            report.ReportPath = await _reportWriter.WriteReport(report);
            return report;
        }

        public string CurrentJobRunnerTask
        {
            get => _currentJobRunnerFile ?? "";
            set => SetProperty(ref _currentJobRunnerFile, value);
        }

        public int JobRunnerProgress
        {
            get => _jobRunnerProgress;
            set
            {
                SetProperty(ref _jobRunnerProgress, value);
                InvokePropertyChanged(nameof(JobRunnerProgressPercent));
            }
        }

        public string JobRunnerProgressPercent
        {
            get => $"{JobRunnerProgress}%";
        }

        private void JobRunner_CurrentTaskChanged(object? sender, string task)
        {
            _dispatcherService.MainWindowDispatcher?.TryEnqueue(() =>
            {
                CurrentJobRunnerTask = task;
            });

        }

        private void JobRunner_ProgressChanged(object? sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            _dispatcherService.MainWindowDispatcher?.TryEnqueue(() =>
            {
                JobRunnerProgress = e.ProgressPercentage;
            });
        }
        private void LoadSortJobs()
        {
            SortJobs.Clear();

            foreach (var job in _appSettings.SortJobs.OrderBy(x => x.Name))
            {
                SortJobs.Add(job);
            }
            InvokePropertyChanged(nameof(GetIncludeExtensions));
            InvokePropertyChanged(nameof(GetExcludeExtensions));
        }

        private void SaveAppSettings()
        {
            _appSettings.SortJobs = SortJobs;
        }
    }
}
