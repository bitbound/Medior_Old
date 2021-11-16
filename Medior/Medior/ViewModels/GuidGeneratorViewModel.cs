﻿using Medior.Core.Shared.BaseTypes;

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
