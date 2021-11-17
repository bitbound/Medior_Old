using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Medior.Models.Messages
{
    public class SignInStateMessage : ValueChangedMessage<bool>
    {
        public SignInStateMessage(bool isSignedIn) : base(isSignedIn)
        {
        }
    }
}
