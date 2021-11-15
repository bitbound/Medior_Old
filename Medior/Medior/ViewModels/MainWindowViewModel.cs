using Medior.Core.Shared.BaseTypes;
using Medior.Enums;
using Medior.Models;
using Medior.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAppModuleStore _appModuleStore;

        private readonly IAppSettings _appSettings;
        private readonly IAuthService _authService;
        private readonly ILogger<MainWindowViewModel> _logger;
        private bool _isGuestMode;
        private bool _isSignedIn;
        private AppModule? _selectedModule;
        private SubscriptionLevel _subscriptionLevel;
        public MainWindowViewModel(
            IAppSettings appSettings,
            IAppModuleStore appModuleStore,
            IAccountService accountService,
            IAuthService authService,
            ILogger<MainWindowViewModel> logger)
        {
            _appSettings = appSettings;
            _authService = authService;
            _appModuleStore = appModuleStore;
            _logger = logger;
        }

        public ObservableCollectionEx<AppModule> AppModulesFooter { get; } = new();

        public ObservableCollectionEx<AppModule> AppModulesMain { get; } = new();

        public bool IsGuestMode
        {
            get => _isGuestMode;
            set
            {
                SetProperty(ref _isGuestMode, value);
                InvokePropertyChanged(nameof(IsMainNavigationVisible));
                InvokePropertyChanged(nameof(IsSignInGridVisible));
            }
        }

        public bool IsMainNavigationVisible => IsSignedIn || IsGuestMode;

        public bool IsSignInGridVisible => !IsMainNavigationVisible;

        public bool IsSignedIn
        {
            get => _isSignedIn;
            set
            {
                SetProperty(ref _isSignedIn, value);
                InvokePropertyChanged(nameof(IsMainNavigationVisible));
                InvokePropertyChanged(nameof(IsSignInGridVisible));
            }
        }

        public async Task<Result<AuthenticationResult>> SignUpSignIn(IntPtr windowHandle)
        {
            var result = await _authService.SignInInteractive(windowHandle);
            IsSignedIn = result.IsSuccess;
            return result;
        }

        public string SearchText { get; set; } = string.Empty;
        public AppModule? SelectedModule
        {
            get => _selectedModule;
            set => SetProperty(ref _selectedModule, value);
        }

        public AppModule SettingsAppModule { get; } = new()
        {
            Label = "Settings",
            PageName = "Medior.Pages.SettingsPage"
        };

        public SubscriptionLevel SubscriptionLevel
        {
            get => _subscriptionLevel;
            set => SetProperty(ref _subscriptionLevel, value);
        }

        public async Task LoadAuthState(IntPtr windowHandle)
        {
            var result = await _authService.GetTokenSilently(windowHandle);
            IsSignedIn = result.IsSuccess;
            // TODO: Use messaging service to broadcast sign-in state changes.
        }
        
        public void FilterModules(string searchText)
        {
            try
            {
                foreach (var module in AppModulesMain.Where(x => x.IsEnabled))
                {
                    module.IsShown = module.Label.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase);
                }
                AppModulesMain.InvokeCollectionChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering modules.");
            }
        }

        public void LoadMenuItems()
        {
            try
            {
                AppModulesMain.Clear();
                AppModulesFooter.Clear();

                foreach (var mainModule in _appModuleStore.MainModules)
                {
                    AppModulesMain.Add(mainModule);
                }

                foreach (var mainModule in _appModuleStore.FooterModules)
                {
                    AppModulesFooter.Add(mainModule);
                }

                SelectedModule = AppModulesMain.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu items.");
            }
        }
    }
}
