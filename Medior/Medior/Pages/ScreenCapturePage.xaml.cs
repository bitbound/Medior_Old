using CommunityToolkit.Diagnostics;
using Medior.Controls;
using Medior.Extensions;
using Medior.Utilities;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.IO;

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

        public AsyncRelayCommand CaptureVideo => new(async () =>
        {
            Guard.IsNotNull(MainWindow.Instance, nameof(MainWindow.Instance));

            var displayPicker = new DisplayPicker();

            var dialogResult = await this.ShowDialog("Select a display to capture", displayPicker);
            if (dialogResult != ContentDialogResult.Primary)
            {
                return;
            }

            var selectedDisplay = displayPicker.SelectedDisplay;

            if (selectedDisplay is null)
            {
                return;
            }
            
            var filename = $"{DateTime.Now:yyyyMMdd-HHmm-ss}.mp4";
            
            var filePath = Path.Combine(AppFolders.RecordingsPath, filename);

            var result = await ViewModel.StartVideoCapture(selectedDisplay, filePath);

            Process.Start(new ProcessStartInfo()
            {
                FileName = filePath,
                UseShellExecute = true,
            });

            if (!result.IsSuccess)
            {
                await this.Alert("Capture Error", "An error occurred while capturing the video.");
                return;
            }

            // TODO: Save the temp video.
        });

        private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.UnregisterSubscriptions();
        }
    }
}
