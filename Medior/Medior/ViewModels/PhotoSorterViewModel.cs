using Medior.Core.Shared.BaseTypes;
using Medior.Core.PhotoSorter.Models;
using Medior.Core.PhotoSorter.Services;
using System;
using System.Linq;
using Medior.Core.Shared.Extensions;
using Microsoft.Extensions.Logging;
using Medior.Services;
using Medior.Core.PhotoSorter.Enums;

namespace Medior.ViewModels
{
    public class PhotoSorterViewModel : ViewModelBase
    {
        private readonly IAppSettings _appSettings;
        private readonly IPathTransformer _pathTransformer;
        private readonly IJobRunner _jobRunner;
        private readonly ILogger<PhotoSorterViewModel> _logger;
        private SortJob? _selectedJob;

        public PhotoSorterViewModel(
            IAppSettings appSettings,
            IPathTransformer pathTransformer,
            IJobRunner jobRunner,
            ILogger<PhotoSorterViewModel> logger)
        {
            _appSettings = appSettings;
            _pathTransformer = pathTransformer;
            _jobRunner = jobRunner;
            _logger = logger;
            
            LoadSortJobs();
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
