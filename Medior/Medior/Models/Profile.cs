using Medior.AppModules.PhotoSorter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    public class Profile
    {
        public DateTimeOffset LastSaved { get; set; }
        public List<SortJob> SortJobs { get; init; } = new();
        public bool IsCloudSyncEnabled { get; set; }
    }
}
