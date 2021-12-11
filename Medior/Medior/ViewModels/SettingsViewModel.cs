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
        private readonly IMessagePublisher _messagePublisher;

        public SettingsViewModel(
            IMessagePublisher messagePublisher, 
            IAuthService authService,
            IApiService apiService)
        {
            _messagePublisher = messagePublisher;
            _authService = authService;
            _apiService = apiService;

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
        }

        public string Email => _authService.Email;

        public void SignOut()
        {
            _authService.SignOut();
        }

        public async Task<Result> SignIn(IntPtr hwnd)
        {
            return await _apiService.TrySignIn(hwnd);
        }
    }
}
