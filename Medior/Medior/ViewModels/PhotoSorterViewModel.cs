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

namespace Medior.ViewModels
{
    public class PhotoSorterViewModel : ViewModelBase
    {
        private readonly IAppSettings _appSettings;
        private readonly IPathTransformer _pathTransformer;
        private readonly IJobRunner _jobRunner;
        private readonly IReportWriter _reportWriter;
        private readonly ILogger<PhotoSorterViewModel> _logger;
        private SortJob? _selectedJob;
        private bool _isDryRun;
        private bool _isJobRunning;

        public PhotoSorterViewModel(
            IAppSettings appSettings,
            IPathTransformer pathTransformer,
            IJobRunner jobRunner,
            IReportWriter reportWriter,
            ILogger<PhotoSorterViewModel> logger)
        {
            _appSettings = appSettings;
            _pathTransformer = pathTransformer;
            _jobRunner = jobRunner;
            _reportWriter = reportWriter;
            _logger = logger;
            
            LoadSortJobs();
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


        public ObservableCollectionEx<SortJob> SortJobs { get; } = new();

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
            var report = await _jobRunner.RunJob(SelectedJob, IsDryRun);
            report.ReportPath = await _reportWriter.WriteReport(report);
            return report;
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
