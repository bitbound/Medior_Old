namespace Medior.Services
{
    public interface IMessagePublisher
    {
        IMessenger Messenger { get; }
    }

    public class MessagePublisher : IMessagePublisher
    {
        public IMessenger Messenger => StrongReferenceMessenger.Default;
    }
}
