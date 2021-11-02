using Medior.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class SorterPageViewModel : ViewModelBase
    {
        private readonly IJobRunner _jobRunner;

        public SorterPageViewModel(IJobRunner jobRunner)
        {
            _jobRunner = jobRunner;
        }
    }
}
