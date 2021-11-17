using CommunityToolkit.Diagnostics;
using Medior.Extensions;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            this.InitializeComponent();
        }

        public DashboardViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<DashboardViewModel>();

        public AsyncRelayCommand SignIn => new(async () =>
        {
            Guard.IsNotNull(MainWindow.Instance, nameof(MainWindow.Instance));
            await ViewModel.SignIn(MainWindow.Instance.GetWindowHandle());
        });

        public RelayCommand SignOut => new(() =>
        {
            ViewModel.SignOut();
        });
    }
}
