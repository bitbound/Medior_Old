using Medior.BaseTypes;
using Medior.Enums;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Medior.Models
{
    public class AppModule : ViewModelBase
    {
        private bool _isEnabled;
        private bool _isShown = true;
        public string Description { get; set; } = string.Empty;
        public string? FontIcon { get; set; }
        public Symbol Icon { get; set; }
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

        public Guid Id { get; set; } = Guid.Empty;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        public bool IsShown
        {
            get => _isShown;
            set => SetProperty(ref _isShown, value);
        }

        public string Label { get; set; } = string.Empty;
        public AppModuleType ModuleType { get; set; }
        public string PageName { get; set; } = string.Empty;
    }

}
