using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Settings;

namespace ClippyDo.Adapter.Sqlite;

internal sealed class SqliteSettingsStore : ISettingsStore
{
    public Task<Settings> LoadAsync(CancellationToken ct = default) => Task.FromResult(new Settings());
    public Task SaveAsync(Settings settings, CancellationToken ct = default) => Task.CompletedTask;
}