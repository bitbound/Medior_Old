using Medior.Core.PhotoSorter.Services;
using Medior.Core.Shared.Services;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public static class ServiceContainer
    {
        public static void Build()
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
            serviceCollection.AddScoped<IProcessEx, ProcessEx>();
            serviceCollection.AddSingleton<IChrono, Chrono>();
            serviceCollection.AddSingleton<ISorterState>(new SorterState()
            {
                ConfigPath = string.Empty,
                DryRun = false,
                JobName = string.Empty,
                Once = true
            });

            serviceCollection.AddScoped<MainWindowViewModel>();
            serviceCollection.AddScoped<SorterPageViewModel>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var services = serviceProvider.GetRequiredService<IServiceProvider>();
            serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .AddProvider(new FileLoggerProvider(services));

            Ioc.Default.ConfigureServices(serviceProvider);
        }
    }
}
