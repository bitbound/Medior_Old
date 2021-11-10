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
                Id = AppModuleIds.ScreenCapture,
                Label = "Screen Capture",
                Description = "Capture and share images and videos of your desktop",
                PageName = "ScreenCapturePage",
                Icon = Symbol.Camera,
                ModuleType = AppModuleType.Main
            },
            new()
            {
                Id = AppModuleIds.RemoteHelp,
                Label = "Remote Help",
                Description = "Give or receive PC support by sharing desktops",
                PageName = "RemoteHelpPage",
                FontIcon = "\xE703",
                ModuleType = AppModuleType.Main
            },
            new()
            {
                Id = AppModuleIds.PhotoSorter,
                Label = "Photo Sorter",
                Description = "Sort photos and videos into folders based on EXIF data",
                PageName = "PhotoSorterPage",
                Icon = Symbol.BrowsePhotos,
                ModuleType = AppModuleType.Main
            },
            new()
            {
                Id = AppModuleIds.QrCodeCreator,
                Label = "QR Code Creator",
                Description = "Generate QR codes to scan with your phone",
                PageName = "PhotoSorterPage",
                FontIcon = "\xED14",
                ModuleType = AppModuleType.Main
            },
            new()
            {
                Id = AppModuleIds.GuidGenerator,
                Label = "GUID Generator",
                Description = "Quickly generate GUIDs",
                PageName = "GuidGeneratorPage",
                FontIcon = "\xE943",
                ModuleType = AppModuleType.Main
            },
            new()
            {
                Id = AppModuleIds.About,
                Label = "About",
                Description = "Information about this app",
                PageName = "AboutPage",
                Icon = Symbol.Help,
                ModuleType = AppModuleType.Footer
            }
        };
    }
}
