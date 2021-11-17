using Medior.PhotoSorter.Models;

namespace Medior.Models
{
    public class MediorConfig
    {
        public IList<SortJob> SortJobs { get; set; } = new List<SortJob>();
        public IList<AppModule> AppModules { get; set; } = new List<AppModule>();
    }
}
