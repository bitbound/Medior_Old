using Medior.Extensions;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ElevatorPage : Page
    {
        private AsyncRelayCommand? _launchAsSystem;
        private AsyncRelayCommand<string>? _launchShell;

        public ElevatorPage()
        {
            this.InitializeComponent();
        }

        public ElevatorViewModel ViewModel => Ioc.Default.GetRequiredService<ElevatorViewModel>();

        public AsyncRelayCommand LaunchAsSystem
        {
            get
            {
                if (_launchAsSystem is null)
                {
                    _launchAsSystem = new(async () =>
                    {
                        try
                        {
                            var result = await ViewModel.LaunchAsSystem(ViewModel.CommandLine ?? "");

                            if (!result.IsSuccess)
                            {
                                await this.Alert("Launch Failure", "There was an error while trying to launch the process.");
                            }
                        }
                        catch
                        {
                            await this.Alert("Launch Failure", "There was an error while trying to launch the process.");
                        }
                    },
                    () => !string.IsNullOrWhiteSpace(ViewModel.CommandLine));
                }
                return _launchAsSystem;
            }
        }

        public AsyncRelayCommand<string> LaunchShell
        {
            get
            {
                if (_launchShell is null)
                {
                    
                    _launchShell = new(async (param) =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(param))
                            {
                                return;
                            }

                            var result = await ViewModel.LaunchAsSystem(param);

                            if (!result.IsSuccess)
                            {
                                await this.Alert("Launch Failure", "Failed to launch shell.  Make sure it's in your %path% variable.");
                            }
                        }
                        catch
                        {
                            await this.Alert("Launch Failure", "Failed to launch shell.  Make sure it's in your %path% variable.");
                        }
                    });
                }
                return _launchShell;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LaunchAsSystem.NotifyCanExecuteChanged();
        }
    }
}
