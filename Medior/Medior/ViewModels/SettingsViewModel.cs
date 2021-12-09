using Medior.BaseTypes;
using Medior.Models.Messages;
using Medior.Services;

namespace Medior.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IMessagePublisher _messagePublisher;

        public SettingsViewModel(IMessagePublisher messagePublisher, IAuthService authService)
        {
            _messagePublisher = messagePublisher;
            _authService = authService;

            _messagePublisher.Messenger.Register<SignInStateMessage>(this, (r, m) =>
            {
                IsSignedIn = m.Value;
                InvokePropertyChanged(nameof(Email));
            });
        }

        public string Email => _authService.Email;
    }
}
