using Medior.Core.PhotoSorter.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class SorterPageViewModel : ObservableObject
    {
        private readonly IJobRunner _jobRunner;

        public SorterPageViewModel(IJobRunner jobRunner)
        {
            _jobRunner = jobRunner;
        }
    }
}
