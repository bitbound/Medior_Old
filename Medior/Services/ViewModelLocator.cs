using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Medior.Core.Shared.Services;
using Medior.ViewModels;
using System;
using Medior.Core.PhotoSorter.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace Medior.Services
{
    public class ViewModelLocator
    {
        public MainWindowViewModel MainWindowViewModel => Ioc.Default.GetRequiredService<MainWindowViewModel>();
        public SorterPageViewModel SorterPageViewModel => Ioc.Default.GetRequiredService<SorterPageViewModel>();
    }
}
