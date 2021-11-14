using Microsoft.UI.Dispatching;

namespace Medior.Services
{
    public interface IDispatcherService
    {
        DispatcherQueue? MainWindowDispatcher { get; }
    }

    public class DispatcherService : IDispatcherService
    {
        public DispatcherQueue? MainWindowDispatcher => MainWindow.Instance?.DispatcherQueue;
    }
}
