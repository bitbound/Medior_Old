using Medior.Core.Shared.BaseTypes;
using Medior.Enums;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
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

        public Guid Id { get; set; } = Guid.Empty;
        public bool IsFavorited
        {
            get => _isFavorited;
            set => SetProperty(ref _isFavorited, value);
        }
        public bool IsProOnly { get; set; }
        public bool IsShown
        {
            get => _isShown;
            set => SetProperty(ref _isShown, value);
        }
        public string Label { get; set; } = string.Empty;
        public AppModuleType ModuleType { get; set; }
        public string PageName { get; set; } = string.Empty;
        public Symbol Icon { get; set; }
        public string? FontIcon { get; set; }
        public IconElement IconElement
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FontIcon))
                {
                    return new FontIcon()
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = FontIcon
                    };
                }
                else
                {
                    return new SymbolIcon(Icon);
                }
            }
        }
        public string Description { get; set; } = string.Empty;
    }

}
