using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Core.Abstractions;

public interface IClipRepository
{
    Task UpsertAsync(Clip clip, CancellationToken ct = default);
    Task<Clip?> GetByIdAsync(ClipId id, CancellationToken ct = default);
    IAsyncEnumerable<Clip> GetRecentAsync(int take, CancellationToken ct = default);
    Task PinAsync(ClipId id, bool isPinned, CancellationToken ct = default);
}