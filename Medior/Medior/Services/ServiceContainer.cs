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
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddScoped<IMetadataReader, MetadataReader>();
            collection.AddScoped<IJobRunner, JobRunner>();
            collection.AddSingleton<IJobWatcher, JobWatcher>();
            collection.AddScoped<IPathTransformer, PathTransformer>();
            collection.AddScoped<IFileSystem, FileSystem>();
            collection.AddScoped<IReportWriter, ReportWriter>();
            collection.AddScoped<IConfigService, ConfigService>();
            collection.AddScoped<IProcessEx, ProcessEx>();
            collection.AddSingleton<IChrono, Chrono>();
            collection.AddSingleton<ISorterState>(new SorterState()
            {
                ConfigPath = string.Empty,
                DryRun = false,
                JobName = string.Empty,
                Once = true
            });

            collection.AddScoped<MainWindowViewModel>();
            collection.AddScoped<SorterPageViewModel>();

            var serviceProvider = collection.BuildServiceProvider();

            var services = serviceProvider.GetRequiredService<IServiceProvider>();
            serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .AddProvider(new FileLoggerProvider(services));

            Ioc.Default.ConfigureServices(serviceProvider);
        }
    }
}
