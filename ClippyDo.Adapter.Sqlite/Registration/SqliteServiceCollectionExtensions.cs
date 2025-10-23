using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Sqlite.Registration;

public static class SqliteServiceCollectionExtensions
{
    public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, string databasePath)
        => services
            .AddSqliteRepository(databasePath)
            .AddSqliteSearch()
            .AddSqliteSettings()
            .AddSqliteMigrations();
}