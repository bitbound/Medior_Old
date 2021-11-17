using Medior.BaseTypes;
using Medior.Models.Messages;
using Medior.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.UI;

namespace Medior.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IMessagePublisher _messagePublisher;
        private double _storagePercent = .45;

        public DashboardViewModel(
            IAuthService authService, 
            IMessagePublisher messagePublisher)
        {
            _authService = authService;
            _messagePublisher = messagePublisher;

            RegisterMessageHandlers();
        }

        public string AccountButtonContent
        {
            get => IsSignedIn ? Email : "Logged Out";
        }

        public string Email => _authService.Email;

        public async Task SignIn(IntPtr windowHandle)
        {
            await _authService.SignInInteractive(windowHandle);
        }

        public void SignOut()
        {
            _authService.SignOut();
        }

        public double StoragePercent
        {
            get => _storagePercent;
            set => SetProperty(ref _storagePercent, value);
        }

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

        public string FormattedStoragePercent => $"{Math.Round(StoragePercent * 100)}%";

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
