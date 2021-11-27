using Medior.Service;
using Medior.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Medior_Service";
    })
    .UseConsoleLifetime()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Main>();
    })
    .ConfigureLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddConsole();
    })
    .Build();

host.Services
    .GetRequiredService<ILoggerFactory>()
    .AddProvider(new FileLoggerProvider(host.Services, "Medior_Service"));

await host.RunAsync();