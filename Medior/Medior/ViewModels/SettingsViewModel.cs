using Medior.BaseTypes;
using Medior.Enums;
using Medior.Models.Messages;
using Medior.Services;
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private readonly IAccountService _accountService;
        private readonly IMessagePublisher _messagePublisher;
        private SubscriptionLevel _subscriptionLevel;
        private string? _unlockCode;

        public SettingsViewModel(
            IMessagePublisher messagePublisher, 
            IAuthService authService,
            IAccountService accountService,
            IApiService apiService)
        {
            _messagePublisher = messagePublisher;
            _authService = authService;
            _apiService = apiService;
            _accountService = accountService;

            _accountService.GetSubscriptionLevel().ContinueWith(x =>
            {
                SubscriptionLevel = x.Result.Value;
            });

            IsSignedIn = _authService.IsSignedIn;
            RegisterSubscriptions();
        }

        private void RegisterSubscriptions()
        {
            _messagePublisher.Messenger.Register<SignInStateMessage>(this, (r, m) =>
            {
                IsSignedIn = m.Value;
                InvokePropertyChanged(nameof(Email));
            });
            _messagePublisher.Messenger.Register<SubscriptionMessage>(this, (r, m) =>
            {
                SubscriptionLevel = m.Value;
            });
        }

        public async Task<Result> UpgradeToPro()
        {
            var result = await _accountService.PurchaseProSubscription();
            if (result.IsSuccess)
            {
                _messagePublisher.Messenger.Send(new SubscriptionMessage(SubscriptionLevel.Pro1));
            }
            return result;
        }

        public string Email => _authService.Email;

        public void SignOut()
        {
            _authService.SignOut();
        }

        public SubscriptionLevel SubscriptionLevel
        {
            get => _subscriptionLevel;
            set => SetProperty(ref _subscriptionLevel, value);
        }

        public Visibility UpgradeVisibility
        {
            get
            {
                return _subscriptionLevel == SubscriptionLevel.Free ?
                    Visibility.Visible :
                    Visibility.Collapsed;
            }
        }

        public string? UnlockCode
        {
            get => _unlockCode;
            set => SetProperty(ref _unlockCode, value);
        }

        public async Task<Result> SignIn(IntPtr hwnd)
        {
            return await _apiService.TrySignIn(hwnd);
        }
    }
}
