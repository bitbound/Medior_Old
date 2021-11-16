using Medior.Core.Shared.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class GuidGeneratorViewModel : ViewModelBase
    {
        private string? _currentGuid;

        public string? CurrentGuid
        {
            get => _currentGuid;
            set => SetProperty(ref _currentGuid, value);
        }
    }
}
