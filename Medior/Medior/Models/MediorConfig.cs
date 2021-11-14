using Medior.Core.PhotoSorter.Models;
using System.Collections.Generic;

namespace Medior.Models
{
    public class MediorConfig
    {
        public IList<SortJob> SortJobs { get; set; } = new List<SortJob>();
        public IList<AppModule> AppModules { get; set; } = new List<AppModule>();
    }
}
