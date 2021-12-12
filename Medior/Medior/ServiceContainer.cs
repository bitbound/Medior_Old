using Medior.AppModules.PhotoSorter.Services;
using Medior.AppModules.ScreenCapture.Services;
using Medior.Services;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace Medior
{
    public static class ServiceContainer
    {
        private static bool _isBuilt;

        public static void Build()
        {
            if (_isBuilt)
            {
                return;
            }

            _isBuilt = true;
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddHttpClient();
            collection.AddScoped<IMetadataReader, MetadataReader>();
            collection.AddScoped<IJobRunner, JobRunner>();
            collection.AddScoped<IPathTransformer, PathTransformer>();
            collection.AddScoped<IFileSystem, FileSystem>();
            collection.AddScoped<IReportWriter, ReportWriter>();
            collection.AddScoped<IProcessEx, ProcessEx>();
            collection.AddScoped<IMessagePublisher, MessagePublisher>();
            collection.AddScoped<IApiService, ApiService>();
            collection.AddScoped<IResourceExtractor, ResourceExtractor>();
            collection.AddScoped<IScreenGrabber, ScreenGrabber>();
            collection.AddScoped<IScreenRecorder, ScreenRecorder>();
            collection.AddScoped<IScreenCapturer, ScreenCapturer>();
            collection.AddSingleton<IAppModuleStore, AppModuleStore>();
            collection.AddSingleton<IChrono, Chrono>();
            collection.AddSingleton<IDispatcherService, DispatcherService>();
            collection.AddSingleton<IProfileService, ProfileService>();
            collection.AddSingleton<IAuthService, AuthService>();
            collection.AddSingleton<ISorterState>(new SorterState()
            {
                ConfigPath = string.Empty,
                DryRun = false,
                JobName = string.Empty,
                Once = true
            });

            collection.AddSingleton<MainWindowViewModel>();
            collection.AddSingleton<PhotoSorterViewModel>();
            collection.AddSingleton<RemoteHelpViewModel>();
            collection.AddSingleton<ScreenCaptureViewModel>();
            collection.AddSingleton<SettingsViewModel>();
            collection.AddSingleton<GuidGeneratorViewModel>();
            collection.AddSingleton<ElevatorViewModel>();
            collection.AddSingleton<QrCodeCreatorViewModel>();

            var instance = collection.BuildServiceProvider();

            instance
                .GetRequiredService<ILoggerFactory>()
                .AddProvider(new FileLoggerProvider(instance));

            Ioc.Default.ConfigureServices(instance);
        }
    }
}
