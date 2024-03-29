﻿global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using Microsoft.Toolkit.Mvvm.Messaging;
using Medior.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow? _mainWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            ServiceContainer.Build();
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            UnhandledException += App_UnhandledException;

            var lastArg = Environment.CommandLine.Split(" ").Last();
            if (Uri.TryCreate(lastArg, UriKind.Absolute, out var uri) && 
                uri.Scheme == "medior")
            {
                // TODO: Handle Uri.
            }

            _mainWindow = new MainWindow
            {
                ExtendsContentIntoTitleBar = true
            };

            _mainWindow.SetTitleBar(_mainWindow.CustomTitleBar);
            
            _mainWindow.Activate();
        }
        
        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var logger = Ioc.Default.GetRequiredService<ILogger<App>>();
            logger.LogError(e.Exception, "An unhandled exception occurred.");
        }

    }
}
