using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace Medior.Services
{
    public static class ViewModelLocator
    {
        public static MainWindowViewModel MainWindowViewModel => Ioc.Default.GetRequiredService<MainWindowViewModel>();
        public static PhotoSorterViewModel SorterPageViewModel => Ioc.Default.GetRequiredService<PhotoSorterViewModel>();
    }
}
