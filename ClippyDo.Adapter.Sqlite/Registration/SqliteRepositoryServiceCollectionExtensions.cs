using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Sqlite.Registration;

public static class SqliteRepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteRepository(this IServiceCollection services, string databasePath)
    {
        services.AddSingleton(new SqliteOptions(databasePath));
        services.AddSingleton<IClipRepository, SqliteClipRepository>();
        return services;
    }
}