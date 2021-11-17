using CommunityToolkit.Diagnostics;
using Medior.Extensions;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScreenCapturePage : Page
    {
        public ScreenCapturePage()
        {
            InitializeComponent();
            ViewModel.RegisterSubscriptions();
        }

        public ScreenCaptureViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<ScreenCaptureViewModel>();

        public AsyncRelayCommand CaptureScreenShot => new(async () => {
            Guard.IsNotNull(MainWindow.Instance, nameof(MainWindow.Instance));

            MainWindow.Instance.Minimize();

            ViewModel.StartScreenClip();

            var clipProc = Process.GetProcessesByName("ScreenClippingHost").FirstOrDefault();
            if (clipProc is not null)
            {
                await clipProc.WaitForExitAsync();
            }

            MainWindow.Instance.Restore();
        });

        public RelayCommand CaptureVideo => new(() => ViewModel.StartVideoCapture());

        private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.UnregisterSubscriptions();
        }
    }
}
