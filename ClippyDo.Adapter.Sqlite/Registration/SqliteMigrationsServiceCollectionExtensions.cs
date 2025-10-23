using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Sqlite.Registration;

public static class SqliteMigrationsServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteMigrations(this IServiceCollection services)
    {
        services.AddSingleton<IStartupTask, SqliteMigrationsTask>();
        return services;
    }
}