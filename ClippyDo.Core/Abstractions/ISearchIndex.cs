using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Core.Abstractions;

public interface ISearchIndex
{
    IAsyncEnumerable<Clip> SearchAsync(string query, CancellationToken ct = default);
}