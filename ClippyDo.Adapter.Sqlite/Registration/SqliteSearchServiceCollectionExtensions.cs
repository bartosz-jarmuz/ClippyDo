using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Sqlite.Registration;

public static class SqliteSearchServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteSearch(this IServiceCollection services)
    {
        services.AddSingleton<ISearchIndex, SqliteFtsSearchIndex>();
        return services;
    }
}