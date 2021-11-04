using Medior.Core.BaseTypes;
using Medior.Core.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public class AppModuleStore
    {
        public AppModule[] AllModules { get; } = new[]
        {
            new AppModule()
            {
                Id = AppModuleIds.About,
                Label = "About",
                Tooltip = "Info about Medior"
            },
            new AppModule()
            {
                Id = AppModuleIds.Settings,
                Label = "Settings",
                Tooltip = "Application settings"
            },
            new AppModule()
            {
                Id = AppModuleIds.PhotoSorter,
                Label = "Photo Sorter",
                Tooltip = "Sort photos and videos into folders based on EXIF data"
            },
        };
    }
}
