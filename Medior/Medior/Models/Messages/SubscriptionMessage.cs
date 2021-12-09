using Medior.Enums;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Medior.Models.Messages
{
    public class SubscriptionMessage : ValueChangedMessage<SubscriptionLevel>
    {
        public SubscriptionMessage(SubscriptionLevel subscriptionLevel) : base(subscriptionLevel)
        {
        }
    }
}
