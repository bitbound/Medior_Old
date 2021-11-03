using Medior.Core.PhotoSorter.Models;

namespace Medior.Core.Shared.Models
{
    public class MediorConfig
    {
        public SortJob[] SortJobs { get; init; } = Array.Empty<SortJob>();
    }
}
