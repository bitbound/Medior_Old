using Medior.Extensions;
using Medior.Models;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            Instance = this;

            Title = "Medior";
            this.SetStoreContext();

            InitializeComponent();            
        }

        public static MainWindow? Instance { get; private set; }
        public UIElement CustomTitleBar => TitleBarElement;
        
        public RelayCommand SignInAsGuest => new(() => ViewModel.IsGuestMode = true);

        public RelayCommand SignUpSignIn => new(async () =>
            {
                var result = await ViewModel.SignUpSignIn(this.GetWindowHandle());
                if (!result.IsSuccess)
                {
                    await RootGrid.Alert("Authentication Failed", result.Error ?? "Sign-in process failed.");
                }
            });

        public MainWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<MainWindowViewModel>();

        private void AppModuleSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.FilterModules(sender.Text);
        }

        private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            AppModuleSearch.Focus(FocusState.Programmatic);
        }
        private void LoadSelectedModule()
        {
            var pageName = $"Medior.Pages.{ViewModel.SelectedModule?.PageName}";
            var pageType = Type.GetType(pageName);
            if (pageType is not null)
            {
                NavigationFrame.Navigate(pageType);
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            
            if (args.IsSettingsSelected)
            {
                ViewModel.SelectedModule = ViewModel.SettingsAppModule;
            }
            else if (args.SelectedItem is AppModule appModule)
            {
                ViewModel.SelectedModule = appModule;
            }
            else
            {
                return;
            }

            LoadSelectedModule();
        }

        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadMenuItems();
            LoadSelectedModule();
            await ViewModel.LoadAuthState(this.GetWindowHandle());
            ViewModel.IsLoading = false;
        }
    }
}
