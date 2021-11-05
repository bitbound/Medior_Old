using Medior.Core.BaseTypes;
using Medior.Core.PhotoSorter.Models;
using Medior.Core.PhotoSorter.Services;
using Medior.Core.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class PhotoSorterViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IJobRunner _jobRunner;
        private SortJob? _selectedJob;

        public PhotoSorterViewModel(IConfigService configService, IJobRunner jobRunner)
        {
            _configService = configService;
            _jobRunner = jobRunner;

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

        public void DeleteSortJob()
        {
            _configService.Current.SortJobs.RemoveAll(x => x.Id == SelectedJob?.Id);
            _configService.SaveConfig();
            LoadSortJobs();
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
