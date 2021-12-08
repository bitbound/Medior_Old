using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models.Messages
{
    internal class IsLoadingMessage : ValueChangedMessage<bool>
    {
        public IsLoadingMessage(bool value) : base(value)
        {
        }
    }
}
