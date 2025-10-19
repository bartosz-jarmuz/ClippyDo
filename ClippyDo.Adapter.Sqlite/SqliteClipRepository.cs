using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Adapter.Sqlite;

internal sealed class SqliteClipRepository : IClipRepository
{
    public Task<Clip?> GetByIdAsync(ClipId id, CancellationToken ct = default) => Task.FromResult<Clip?>(null);
    public Task PinAsync(ClipId id, bool isPinned, CancellationToken ct = default) => Task.CompletedTask;
    public async IAsyncEnumerable<Clip> GetRecentAsync(int take, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default) { yield break; }
    public Task UpsertAsync(Clip clip, CancellationToken ct = default) => Task.CompletedTask;
}