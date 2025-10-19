using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Core.Services;

public static class OrderingPolicy
{
    public static IOrderedEnumerable<Clip> OrderForDisplay(IEnumerable<Clip> clips)
        => clips
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.LastUsedAtUtc);
}