using Medior.Core.PhotoSorter.Models;

namespace Medior.Core.Shared.Models
{
    public class MediorConfig
    {
        public List<SortJob> SortJobs { get; set; } = new();
        public List<Guid> FavoriteModules { get; set; } = new();
    }
}
