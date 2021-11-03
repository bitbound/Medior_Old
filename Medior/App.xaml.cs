using Medior.Core.ScreenCapture.Helpers;
using Medior.Services;
using System;
using System.Windows;
using Windows.System;

namespace Medior
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#pragma warning disable IDE0052 // Remove unread private members
        private DispatcherQueueController? _controller;
#pragma warning restore IDE0052 // Remove unread private members

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _controller = CoreMessagingHelper.CreateDispatcherQueueControllerForCurrentThread();
            ServiceContainer.Build();
        }
    }
}
