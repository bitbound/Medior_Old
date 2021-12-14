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
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IProfileService _profileService;
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

        public string Email => _authService.Email;

        public bool IsCloudSyncEnabled
        {
            get => _profileService.Profile.IsCloudSyncEnabled;
            set
            {
                _profileService.Profile.IsCloudSyncEnabled = value;
                _ = _profileService.Save();
                InvokePropertyChanged(nameof(IsCloudSyncEnabled));
            }
        }

        public async Task<Result> ExportProfile(string path)
        {
            return await _profileService.Export(path);
        }

        public async Task<Result> ImportProfile(string path)
        {
            return await _profileService.Import(path);
        }

        public async Task<Result> SignIn(IntPtr hwnd)
        {
            return await _apiService.TrySignIn(hwnd);
        }

        public async Task SignOut()
        {
            await _authService.SignOut(true);
        }

        private void RegisterSubscriptions()
        {
            _messagePublisher.Messenger.Register<SignInStateMessage>(this, (r, m) =>
            {
                IsSignedIn = m.Value;
                InvokePropertyChanged(nameof(Email));
            });
        }
    }
}
