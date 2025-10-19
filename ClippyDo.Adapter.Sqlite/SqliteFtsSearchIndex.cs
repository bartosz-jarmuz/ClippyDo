using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Adapter.Sqlite;

internal sealed class SqliteFtsSearchIndex : ISearchIndex
{
    public async IAsyncEnumerable<Clip> SearchAsync(string query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    { yield break; }
}