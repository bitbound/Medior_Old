using Medior.BaseTypes;
using Medior.Models.Messages;
using Medior.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly IMessagePublisher _messagePublisher;
        private double _storagePercent = .45;

        public DashboardViewModel(
            IAuthService authService,
            IApiService apiService,
            IMessagePublisher messagePublisher)
        {
            _apiService = apiService;
            _authService = authService;
            _messagePublisher = messagePublisher;

            RegisterMessageHandlers();
        }

        public string AccountButtonContent
        {
            get => IsSignedIn ? Email : "Signed Out";
        }

        public string Email => _authService.Email;

        public string FormattedStoragePercent => $"{Math.Round(StoragePercent * 100)}%";

        public Brush StorageColor
        {
            get
            {
                if (_storagePercent <= .5)
                {
                    return new SolidColorBrush(Colors.SpringGreen);
                }

                if (_storagePercent <= .75)
                {
                    return new SolidColorBrush(Colors.Yellow);
                }

                if (_storagePercent <= .9)
                {
                    return new SolidColorBrush(Colors.Orange);
                }

                return new SolidColorBrush(Colors.Red);
            }
        }

        public double StoragePercent
        {
            get => _storagePercent;
            set => SetProperty(ref _storagePercent, value);
        }

        public async Task<Result> SignIn(IntPtr windowHandle)
        {
            var result = await _authService.SignInInteractive(windowHandle);
            if (result.IsSuccess)
            {
                var authResult = await _apiService.TestAuth();
                if (!authResult.IsSuccess)
                {
                    return Result.Fail("Auth token check failed.");
                }

                if (authResult.Value == System.Net.HttpStatusCode.OK)
                {
                    return Result.Ok();
                }

                if (authResult.Value == System.Net.HttpStatusCode.Unauthorized)
                {
                    return Result.Fail("Not authorized.");
                }

                return Result.Fail($"Auth token check returned response code: {authResult.Value}");
            }

            return Result.Fail("Sign-in process failed.");
        }

        public void SignOut()
        {
            _authService.SignOut();
        }
        private void RegisterMessageHandlers()
        {
            _messagePublisher.Messenger.Register<SignInStateMessage>(this, 
                (r, m) =>
                {
                    IsSignedIn = m.Value;
                    InvokePropertyChanged(nameof(AccountButtonContent));
                    InvokePropertyChanged(nameof(Email));
                });
        }
    }
}
