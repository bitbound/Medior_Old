using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.BaseTypes
{
    public static class AppModuleIds
    {
        public static Guid PhotoSorter { get; } = new("83e0a286-eb2d-4a0b-9fa6-61e75cbabffd");
        public static Guid About { get; } = new("1bb0d8a3-73b5-4075-a2ce-2b0232d2fc81");
        public static Guid Settings { get; } = new("4bd293f9-b568-4be3-9b8e-9eb60765cd7d");
    }
}
