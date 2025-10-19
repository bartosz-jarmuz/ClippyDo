using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Sqlite;

internal sealed class SqliteMigrationsTask : IStartupTask
{
    public Task RunAsync(CancellationToken ct = default) => Task.CompletedTask; // TODO: implement migrations
}