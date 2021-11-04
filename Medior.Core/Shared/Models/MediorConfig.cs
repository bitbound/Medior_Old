using Medior.Core.PhotoSorter.Models;

namespace Medior.Core.Shared.Models
{
    public class MediorConfig
    {
        public List<SortJob> SortJobs { get; init; } = new();
        public List<Guid> FavoriteModules { get; init; } = new();
    }
}
