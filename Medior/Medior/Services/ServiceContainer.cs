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
        public static IServiceProvider Instance { get; } = Build();

        private static IServiceProvider Build()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddScoped<IMetadataReader, MetadataReader>();
            collection.AddScoped<IJobRunner, JobRunner>();
            collection.AddScoped<IPathTransformer, PathTransformer>();
            collection.AddScoped<IFileSystem, FileSystem>();
            collection.AddScoped<IReportWriter, ReportWriter>();
            collection.AddScoped<IProcessEx, ProcessEx>();
            collection.AddSingleton<IJobWatcher, JobWatcher>();
            collection.AddSingleton<IAppModuleStore, AppModuleStore>();
            collection.AddSingleton<IChrono, Chrono>();
            collection.AddSingleton<IDispatcherService, DispatcherService>();
            collection.AddSingleton<IAppSettings, AppSettings>();
            collection.AddSingleton<IConfigService, ConfigService>();
            collection.AddSingleton<IAccountService, AccountService>();
            collection.AddSingleton<ISorterState>(new SorterState()
            {
                ConfigPath = string.Empty,
                DryRun = false,
                JobName = string.Empty,
                Once = true
            });

            collection.AddSingleton<MainWindowViewModel>();
            collection.AddSingleton<PhotoSorterViewModel>();
            collection.AddSingleton<SettingsViewModel>();

            var instance = collection.BuildServiceProvider();

            instance
                .GetRequiredService<ILoggerFactory>()
                .AddProvider(new FileLoggerProvider(instance));

            return instance;
        }
    }
}
