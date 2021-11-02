using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Medior.Core.Services;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;

namespace Medior.Cli
{
    public class Program
    {

        public static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Sort your photos into folders based on metadata.");

            var configOption = new Option<string>(
                new[] { "--config-path", "-c" },
                "The full path to the Medior configuration file.  See the readme for an example: https://github.com/lucent-sea/Medior");
            configOption.AddValidator(option =>
            {
                if (File.Exists(option.GetValueOrDefault()?.ToString()))
                {
                    return null;
                }

                return "Config file could not be found at the given path.";
            });
            rootCommand.AddOption(configOption);

            var jobOption = new Option<string>(
                new[] { "--job-name", "-j" },
                () => string.Empty,
                "If specified, will only run the named job from the config, then exit.  Implies --once.");
            rootCommand.AddOption(jobOption);

            var onceOption = new Option<bool>(
                new[] { "--once", "-o" },
                () => false,
                "If true, will run sort jobs immediately, then exit.  If false, will run jobs, then block and monitor for changes in each job's source folder.");
            rootCommand.AddOption(onceOption);

            var dryRunOption = new Option<bool>(
                new[] { "--dry-run", "-d" },
                () => false,
                "If true, no file operations will actually be executed.");
            rootCommand.AddOption(dryRunOption);

            rootCommand.Handler = CommandHandler.Create((string configPath, string jobName, bool once, bool dryRun) =>
            {
                using var host = Host.CreateDefaultBuilder(args)
                    .UseWindowsService(options =>
                    {
                        options.ServiceName = "Medior";
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddScoped<IMetadataReader, MetadataReader>();
                        services.AddScoped<IJobRunner, JobRunner>();
                        services.AddSingleton<IJobWatcher, JobWatcher>();
                        services.AddScoped<IPathTransformer, PathTransformer>();
                        services.AddScoped<IFileSystem, FileSystem>();
                        services.AddSingleton<IChrono, Chrono>();
                        services.AddScoped<IReportWriter, ReportWriter>();
                        services.AddScoped<IConfigService, ConfigService>();
                        services.AddSingleton<IGlobalState>(new GlobalState()
                        {
                            ConfigPath = configPath,
                            DryRun = dryRun,
                            JobName = jobName,
                            Once = once
                        });
                        services.AddHostedService<SortBackgroundService>();
                    })
                    .ConfigureLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.AddConsole();
                    })
                    .Build();

                var services = host.Services.GetRequiredService<IServiceProvider>();
                host.Services
                    .GetRequiredService<ILoggerFactory>()
                    .AddProvider(new FileLoggerProvider(services));

                return host.RunAsync();
            });

            return await rootCommand.InvokeAsync(args);
        }
    }
}
