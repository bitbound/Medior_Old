using Medior.Core.Shared.BaseTypes;
using Medior.Core.Shared.Models;
using Medior.Enums;
using Medior.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IAppModuleStore
    {
        AppModule[] AllModules { get; }
        IEnumerable<AppModule> FooterModules { get; }
        IEnumerable<AppModule> MainModules { get; }
    }

    public class AppModuleStore : IAppModuleStore
    {
        public IEnumerable<AppModule> MainModules =>
            AllModules.Where(x => x.ModuleType == AppModuleType.Main);

        public IEnumerable<AppModule> FooterModules =>
            AllModules.Where(x => x.ModuleType == AppModuleType.Footer);

        public AppModule[] AllModules { get; } = new AppModule[]
        {
            new()
            {
                Id = AppModuleIds.PhotoSorter,
                Label = "Photo Sorter",
                Tooltip = "Sort photos and videos into folders based on EXIF data",
                PageName = "PhotoSorterPage",
                Icon = Symbol.BrowsePhotos,
                ModuleType = AppModuleType.Main
            },
            new()
            {
                Id = AppModuleIds.About,
                Label = "About",
                Tooltip = "Info about Medior",
                PageName = "AboutPage",
                Icon = Symbol.Help,
                ModuleType = AppModuleType.Footer
            }
        };
    }
}
