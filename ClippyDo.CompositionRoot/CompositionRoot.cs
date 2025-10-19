using ClippyDo.Adapter.Sqlite.Registration;
using ClippyDo.Adapter.Windows.Registration;
using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Settings;
using ClippyDo.Core.Services;
using ClippyDo.Infrastructure.Features.Clipboard;
using ClippyDo.Infrastructure.Features.Picker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.CompositionRoot;

public static class CompositionRoot
{
    public static IServiceCollection AddClippyDo(this IServiceCollection services, IConfiguration config)
    {
        // Options/Settings
        var settings = BindSettings(config);
        services.AddSingleton(settings);

        // Core services (domain services)
        services.AddSingleton<TaggingService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IRegexMatcher, SystemRegexMatcher>();
        services.AddSingleton<IHashService, DefaultHashService>();
        services.AddSingleton<ILogger, ConsoleLogger>();

        // Infrastructure – application services
        services.AddSingleton<ClipboardCapturePipeline>();
        services.AddSingleton<SearchCoordinator>();

        // Adapters
        services.AddWindowsAdapters();
        services.AddSqlitePersistence(ExpandEnvVars(settings.DatabasePath));

        return services;
    }

    public static async Task RunStartupTasksAsync(this IServiceProvider provider, CancellationToken ct = default)
    {
        // Run all IStartupTask in deterministic order if needed (here: registration order)
        foreach (var task in provider.GetServices<IStartupTask>())
            await task.RunAsync(ct);

        // Start long-running pipelines
        provider.GetRequiredService<ClipboardCapturePipeline>().Start();
    }

    private static Settings BindSettings(IConfiguration config)
    {
        var s = new Settings();
        config.GetSection("ClippyDo:Settings").Bind(s);
        return s;
    }

    private static string ExpandEnvVars(string path) => Environment.ExpandEnvironmentVariables(path);
}