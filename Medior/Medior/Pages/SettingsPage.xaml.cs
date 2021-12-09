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
        private RelayCommand? _submitUnlockCode;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<SettingsViewModel>();

        private RelayCommand SubmitUnlockCode
        {
            get
            {
                if (_submitUnlockCode is null)
                {
                    _submitUnlockCode = new RelayCommand(() =>
                    {
                        // TODO;
                    },
                    () => !string.IsNullOrWhiteSpace(ViewModel.UnlockCode));
                }
                return _submitUnlockCode;
            }
        }

        private AsyncRelayCommand UpgradeToPro => new(async () =>
        {
            var result = await ViewModel.UpgradeToPro();
            if (result.IsSuccess)
            {
                await this.Alert("Purchase Success", "Subscription purchase completed successfully!");
                // TODO: Publish message of subscription change.
            }
            else
            {
                await this.Alert("Purchase Failed", "Subscription purchase failed.  Please check your network connection or try again later.");
            }
        });

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
        private async void UnlockCodeTextBox_PreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await UpgradeToPro.ExecuteAsync(null);
            }
        }

        private void UnlockCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SubmitUnlockCode?.NotifyCanExecuteChanged();
        }
    }
}
