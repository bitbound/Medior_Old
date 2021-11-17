using Medior.Utilities;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GuidGeneratorPage : Page
    {
        public GuidGeneratorPage()
        {
            this.InitializeComponent();
        }

        public GuidGeneratorViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<GuidGeneratorViewModel>();

        public RelayCommand Copy => new(() =>
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(ViewModel.CurrentGuid);
            Clipboard.SetContent(dataPackage);
            CopiedTip.IsOpen = true;
            Debouncer.Debounce(TimeSpan.FromSeconds(1.5), () =>
            {
                DispatcherQueue.TryEnqueue(() => CopiedTip.IsOpen = false);
            });
        });

        public RelayCommand Refresh => new(() =>
        {
            ViewModel.CurrentGuid = Guid.NewGuid().ToString();
        });
    }
}
