using Medior.Services;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

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
        }

        public ScreenCaptureViewModel ViewModel { get; } = ServiceContainer.Instance.GetRequiredService<ScreenCaptureViewModel>();

        public RelayCommand CaptureScreenShot => new(() => ViewModel.StartScreenClip());
    }
}
