using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Medior.Core.Services;
using Medior.ViewModels;
using System;

namespace Medior.Services
{
    public class ViewModelLocator
    {
        private static readonly ServiceProvider _serviceProvider;

        static ViewModelLocator()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddScoped<IMetadataReader, MetadataReader>();
            serviceCollection.AddScoped<IJobRunner, JobRunner>();
            serviceCollection.AddSingleton<IJobWatcher, JobWatcher>();
            serviceCollection.AddScoped<IPathTransformer, PathTransformer>();
            serviceCollection.AddScoped<IFileSystem, FileSystem>();
            serviceCollection.AddScoped<IReportWriter, ReportWriter>();
            serviceCollection.AddScoped<IConfigService, ConfigService>();
            serviceCollection.AddSingleton<IChrono, Chrono>();
            serviceCollection.AddSingleton<IGlobalState>(new GlobalState()
            {
                ConfigPath = string.Empty,
                DryRun = false,
                JobName = string.Empty,
                Once = true
            });

            serviceCollection.AddScoped<MainWindowViewModel>();
            serviceCollection.AddScoped<SorterPageViewModel>();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var services = _serviceProvider.GetRequiredService<IServiceProvider>();
            _serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .AddProvider(new FileLoggerProvider(services));
        }

        public MainWindowViewModel MainWindowViewModel => _serviceProvider.GetRequiredService<MainWindowViewModel>();
        public SorterPageViewModel SorterPageViewModel => _serviceProvider.GetRequiredService<SorterPageViewModel>();
    }
}
