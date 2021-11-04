using Medior.Core.PhotoSorter.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class PhotoSorterViewModel : ObservableObject
    {
        private readonly IJobRunner _jobRunner;

        public PhotoSorterViewModel(IJobRunner jobRunner)
        {
            _jobRunner = jobRunner;
        }
    }
}
