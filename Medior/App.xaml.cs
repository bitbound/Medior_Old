using Medior.Services;
using System;
using System.Windows;

namespace Medior
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ServiceContainer.Build();
        }
    }
}
