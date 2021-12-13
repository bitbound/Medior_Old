using CommunityToolkit.Diagnostics;
using Medior.Extensions;
using Medior.Services;
using Medior.Utilities;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.IO;
using Windows.Services.Store;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        private AsyncRelayCommand? _exportProfile;

        private AsyncRelayCommand? _importProfile;
        private RelayCommand? _openLogs;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public AsyncRelayCommand ExportProfile
        {
            get
            {
                if (_exportProfile is null)
                {
                    _exportProfile = new(async () =>
                    {
                        var picker = new FileSavePicker
                        {
                            SuggestedStartLocation = PickerLocationId.Desktop
                        };
                        picker.FileTypeChoices.Add("JSON File", new List<string>() { ".json" });
                        MainWindow.Instance?.InitializeObject(picker);
                        var storageItem = await picker.PickSaveFileAsync();
                        if (storageItem is null)
                        {
                            return;
                        }

                        var result = await ViewModel.ExportProfile(storageItem.Path);

                        if (result.IsSuccess)
                        {
                            await this.Alert("Export Successful", "Profile successfully exported.");
                        }
                        else
                        {
                            await this.Alert("Export Failed", result.Error ?? "");
                        }
                    });
                }
                return _exportProfile;
            }
        }

        public AsyncRelayCommand ImportProfile
        {
            get
            {
                if (_importProfile is null)
                {
                    _importProfile = new(async () =>
                    {
                        var picker = new FileOpenPicker
                        {
                            SuggestedStartLocation = PickerLocationId.Desktop
                        };
                        picker.FileTypeFilter.Add(".json");
                        MainWindow.Instance?.InitializeObject(picker);
                        var storageItem = await picker.PickSingleFileAsync();
                        if (storageItem is null)
                        {
                            return;
                        }

                        var result = await ViewModel.ImportProfile(storageItem.Path);

                        if (result.IsSuccess)
                        {
                            await this.Alert("Import Successful", "Profile successfully imported.");
                        }
                        else
                        {
                            await this.Alert("Import Failed", result.Error ?? "");
                        }
                    });
                }
                return _importProfile;
            }
        }

        public RelayCommand OpenLogs
        {
            get
            {
                if (_openLogs is null)
                {
                    _openLogs = new(() =>
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = AppFolders.TempPath,
                            UseShellExecute = true
                        });
                    });
                }
                return _openLogs;
            }
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
