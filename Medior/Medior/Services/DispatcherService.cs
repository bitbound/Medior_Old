using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
