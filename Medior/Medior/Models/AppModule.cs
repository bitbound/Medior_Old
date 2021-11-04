using Medior.Enums;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    public class AppModule
    {
        public Guid Id { get; init; } = Guid.Empty;
        public bool IsFavorited { get; set; }
        public bool IsProOnly { get; init; }
        public string Label { get; init; } = string.Empty;
        public AppModuleType ModuleType { get; init; }
        public string PageName { get; init; } = string.Empty;
        public Symbol Icon { get; init; }
        public string Tooltip { get; init; } = string.Empty;
    }

}
