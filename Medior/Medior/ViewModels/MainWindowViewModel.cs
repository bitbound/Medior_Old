using Medior.BaseTypes;
using Medior.Enums;
using Medior.Models;
using Medior.Models.Messages;
using Medior.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly IAppModuleStore _appModuleStore;
        private readonly IAuthService _authService;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly IMessagePublisher _messagePublisher;
        private bool _isGuestMode;
        private bool _isLoading = true;
        private bool _isSignedIn;
        private AppModule? _selectedModule;
        private SubscriptionLevel _subscriptionLevel;
        public MainWindowViewModel(
            IAppModuleStore appModuleStore,
            IAuthService authService,
            IApiService apiService,
            IMessagePublisher messagePublisher,
            ILogger<MainWindowViewModel> logger)
        {
            _authService = authService;
            _appModuleStore = appModuleStore;
            _apiService = apiService;
            _messagePublisher = messagePublisher;
            _logger = logger;

            RegisterMessageHandlers();
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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsMainNavigationVisible => IsSignedIn || IsGuestMode;

        public override bool IsSignedIn
        {
            get => _isSignedIn;
            set
            {
                SetProperty(ref _isSignedIn, value);
                InvokePropertyChanged(nameof(IsMainNavigationVisible));
                InvokePropertyChanged(nameof(IsSignInGridVisible));
            }
        }

        public bool IsSignInGridVisible => !IsMainNavigationVisible;

        public string SearchText { get; set; } = string.Empty;

        public AppModule? SelectedModule
        {
            get => _selectedModule;
            set => SetProperty(ref _selectedModule, value);
        }

        public AppModule SettingsAppModule { get; } = new()
        {
            Label = "Settings",
            PageName = "SettingsPage"
        };

        public SubscriptionLevel SubscriptionLevel
        {
            get => _subscriptionLevel;
            set => SetProperty(ref _subscriptionLevel, value);
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

        public async Task LoadAuthState(IntPtr windowHandle)
        {
            var result = await _authService.GetTokenSilently(windowHandle, false);
            if (result.IsSuccess)
            {
                await _apiService.TestAuth();
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

        public async Task<Result> SignUpSignIn(IntPtr windowHandle)
        {
            return await _apiService.TrySignIn(windowHandle);
        }

        private void RegisterMessageHandlers()
        {
            _messagePublisher.Messenger.Register<SignInStateMessage>(this, 
                (r,m) =>
                {
                    IsSignedIn = m.Value;
                    IsGuestMode = false;
                });
            _messagePublisher.Messenger.Register<IsLoadingMessage>(this,
                (r, m) => IsLoading = m.Value);
        }
    }
}
