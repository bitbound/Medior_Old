using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Medior.Core.Shared.Services;
using Medior.ViewModels;
using System;
using Medior.Core.PhotoSorter.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace Medior.Services
{
    public static class ViewModelLocator
    {
        public static MainWindowViewModel MainWindowViewModel => Ioc.Default.GetRequiredService<MainWindowViewModel>();
        public static PhotoSorterViewModel SorterPageViewModel => Ioc.Default.GetRequiredService<PhotoSorterViewModel>();
    }
}
