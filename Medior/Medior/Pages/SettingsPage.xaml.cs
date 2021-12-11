using CommunityToolkit.Diagnostics;
using Medior.Extensions;
using Medior.Services;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Windows.Services.Store;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<SettingsViewModel>();

        private async void SignInHyperlink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Guard.IsNotNull(MainWindow.Instance, nameof(MainWindow.Instance));

            var result = await ViewModel.SignIn(MainWindow.Instance.GetWindowHandle());
            if (!result.IsSuccess)
            {
                await this.Alert("Authentication Failed", result.Error ?? "Sign-in process failed.");
            }
        }

        private void SignOutHyperlink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            ViewModel.SignOut();
        }
    }
}
