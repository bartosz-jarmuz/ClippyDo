using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Sqlite.Registration;

public static class SqliteServiceCollectionExtensions
{
    public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, string databasePath)
    {
        // Register concrete implementations that satisfy Core ports
        services.AddSingleton<IClipRepository, SqliteClipRepository>();
        services.AddSingleton<ISearchIndex, SqliteFtsSearchIndex>();
        services.AddSingleton<ISettingsStore, SqliteSettingsStore>();
        services.AddSingleton<IStartupTask, SqliteMigrationsTask>();
        services.AddSingleton(new SqliteOptions(databasePath));
        return services;
    }
}