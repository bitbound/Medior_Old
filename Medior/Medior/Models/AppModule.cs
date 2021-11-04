using Medior.Core.BaseTypes;
using Medior.Enums;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    public class AppModule : ViewModelBase
    {
        private bool _isShown = true;
        private bool _isFavorited;

        public Guid Id { get; init; } = Guid.Empty;
        public bool IsFavorited
        {
            get => _isFavorited;
            set => SetProperty(ref _isFavorited, value);
        }
        public bool IsProOnly { get; init; }
        public bool IsShown
        {
            get => _isShown;
            set => SetProperty(ref _isShown, value);
        }
        public string Label { get; init; } = string.Empty;
        public AppModuleType ModuleType { get; init; }
        public string PageName { get; init; } = string.Empty;
        public Symbol Icon { get; init; }
        public string Tooltip { get; init; } = string.Empty;
    }

}
