using Medior.BaseTypes;
using Medior.Enums;
using Medior.Models;
using Medior.Models.Messages;
using Medior.Services;
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private readonly IProfileService _profileService;
        private readonly IMessagePublisher _messagePublisher;

        public SettingsViewModel(
            IMessagePublisher messagePublisher, 
            IAuthService authService,
            IProfileService profileService,
            IApiService apiService)
        {
            _messagePublisher = messagePublisher;
            _authService = authService;
            _apiService = apiService;
            _profileService = profileService;

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

        public async Task<Result> ImportProfile(string path)
        {
            return await _profileService.Import(path);
        }

        public async Task<Result> ExportProfile(string path)
        {
            return await _profileService.Export(path);
        }
    }
}
