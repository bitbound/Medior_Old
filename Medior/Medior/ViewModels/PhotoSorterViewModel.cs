using Medior.Core.Shared.BaseTypes;
using Medior.Core.PhotoSorter.Models;
using Medior.Core.PhotoSorter.Services;
using Medior.Core.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medior.Core.Shared.Extensions;
using Windows.UI.Notifications;
using Microsoft.Extensions.Logging;

namespace Medior.ViewModels
{
    public class PhotoSorterViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IPathTransformer _pathTransformer;
        private readonly IJobRunner _jobRunner;
        private readonly ILogger<PhotoSorterViewModel> _logger;
        private SortJob? _selectedJob;

        public PhotoSorterViewModel(
            IConfigService configService,
            IPathTransformer pathTransformer,
            IJobRunner jobRunner,
            ILogger<PhotoSorterViewModel> logger)
        {
            _configService = configService;
            _pathTransformer = pathTransformer;
            _jobRunner = jobRunner;
            _logger = logger;
            
            LoadSortJobs();
        }


        public ObservableCollectionEx<SortJob> SortJobs { get; } = new();

        public SortJob? SelectedJob
        {
            get => _selectedJob;
            set => SetProperty(ref _selectedJob, value);
        }


        public void CreateNewSortJob(string sortJobName)
        {
            var sortJob = new SortJob()
            {
                Name = sortJobName
            };

            _configService.Current.SortJobs.Add(sortJob);
            _configService.SaveConfig();

            LoadSortJobs();
            SelectedJob = SortJobs.FirstOrDefault(x => x.Id == sortJob.Id);
        }

        public void DeleteSortJob()
        {
            _configService.Current.SortJobs.RemoveAll(x => x.Id == SelectedJob?.Id);
            _configService.SaveConfig();
            LoadSortJobs();
            SelectedJob = SortJobs.FirstOrDefault();
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

        public void RenameSortJob(string newName)
        {
            var modifiedJob = _configService.Current.SortJobs.Find(x => x.Id == SelectedJob?.Id);
            if (modifiedJob is null)
            {
                return;
            }

            modifiedJob.Name = newName;
            _configService.SaveConfig();
            LoadSortJobs();
            SelectedJob = SortJobs.FirstOrDefault(x => x.Id == modifiedJob.Id);
        }

        public void SaveJob()
        {
            if (_selectedJob is null)
            {
                return;
            }

            if (_configService.Current.SortJobs.TryReplace(_selectedJob))
            {
                _configService.SaveConfig();
            }
        }


        private void LoadSortJobs()
        {
            SortJobs.Clear();

            foreach (var job in _configService.Current.SortJobs.OrderBy(x => x.Name))
            {
                SortJobs.Add(job);
            }
        }

    }
}
