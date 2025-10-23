using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Sqlite.Registration;

public static class SqliteSettingsServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteSettings(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsStore, SqliteSettingsStore>();
        return services;
    }
}