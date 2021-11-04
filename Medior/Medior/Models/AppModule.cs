using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.Shared.Models
{
    public class AppModule
    {
        public Guid Id { get; init; } = Guid.Empty;
        public string Label { get; init; } = string.Empty;
        public string Tooltip { get; internal set; }
    }
}
