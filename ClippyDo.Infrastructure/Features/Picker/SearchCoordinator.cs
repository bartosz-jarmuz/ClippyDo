using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Infrastructure.Features.Picker;

public sealed class SearchCoordinator
{
    private readonly ISearchIndex _search;

    public SearchCoordinator(ISearchIndex search)
    {
        _search = search;
    }

    public async IAsyncEnumerable<Clip> SearchAsync(string query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var c in _search.SearchAsync(query, ct))
            yield return c;
    }
}