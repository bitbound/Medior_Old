global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
using Medior.PhotoSorter.Services;
using Medior.Services;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
            RegisterServices();
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            UnhandledException += App_UnhandledException;

            var lastArg = Environment.CommandLine.Split(" ").Last();
            if (Uri.TryCreate(lastArg, UriKind.Absolute, out var uri) && 
                uri.Scheme == "medior")
            {
                // TODO: Handle Uri.
            }

            var accountService = Ioc.Default.GetRequiredService<IAccountService>();
            var subResult = await accountService.GetSubscriptionLevel();

            _mainWindow = new MainWindow
            {
                ExtendsContentIntoTitleBar = true
            };

            _mainWindow.ViewModel.SubscriptionLevel = subResult.Value;

            _mainWindow.SetTitleBar(_mainWindow.CustomTitleBar);
            
            _mainWindow.Activate();
        }
        
        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var logger = Ioc.Default.GetRequiredService<ILogger<App>>();
            logger.LogError(e.Exception, "An unhandled exception occurred.");
        }

        private void RegisterServices()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddScoped<IMetadataReader, MetadataReader>();
            collection.AddScoped<IJobRunner, JobRunner>();
            collection.AddScoped<IPathTransformer, PathTransformer>();
            collection.AddScoped<IFileSystem, FileSystem>();
            collection.AddScoped<IReportWriter, ReportWriter>();
            collection.AddScoped<IProcessEx, ProcessEx>();
            collection.AddSingleton<IAppModuleStore, AppModuleStore>();
            collection.AddSingleton<IChrono, Chrono>();
            collection.AddSingleton<IDispatcherService, DispatcherService>();
            collection.AddSingleton<IAppSettings, AppSettings>();
            collection.AddSingleton<IAuthService, AuthService>();
            collection.AddSingleton<IAccountService, AccountService>();
            collection.AddSingleton<IEnvironmentService, EnvironmentService>();
            collection.AddSingleton<ISorterState>(new SorterState()
            {
                ConfigPath = string.Empty,
                DryRun = false,
                JobName = string.Empty,
                Once = true
            });

            collection.AddSingleton<MainWindowViewModel>();
            collection.AddSingleton<DashboardViewModel>();
            collection.AddSingleton<PhotoSorterViewModel>();
            collection.AddSingleton<RemoteHelpViewModel>();
            collection.AddSingleton<ScreenCaptureViewModel>();
            collection.AddSingleton<SettingsViewModel>();
            collection.AddSingleton<GuidGeneratorViewModel>();

            var instance = collection.BuildServiceProvider();

            instance
                .GetRequiredService<ILoggerFactory>()
                .AddProvider(new FileLoggerProvider(instance));

            Ioc.Default.ConfigureServices(instance);
        }
    }
}
