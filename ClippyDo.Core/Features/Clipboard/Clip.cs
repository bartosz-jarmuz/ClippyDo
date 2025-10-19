namespace ClippyDo.Core.Features.Clipboard;

public sealed class Clip
{
    public ClipId Id { get; init; } = ClipId.New();
    public ClipKind Kind { get; init; }

    // Text facets
    public string? PlainText { get; init; }
    public byte[]? Rtf { get; init; }
    public byte[]? Html { get; init; }

    // Image facets
    public byte[]? ImageBytes { get; init; }
    public string? ImageFormat { get; init; }
    public byte[]? ThumbnailBytes { get; init; }

    public SourceApp Source { get; init; } = new("unknown", "unknown", null);

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime LastUsedAtUtc { get; private set; } = DateTime.UtcNow;
    public int UsageCount { get; private set; }

    public bool IsPinned { get; private set; }

    public HashSet<string> Tags { get; } = new();

    // CHANGED: make settable so we can assign after hashing
    public ContentHash ContentHash { get; set; }

    public void TouchUsage()
    {
        UsageCount++;
        LastUsedAtUtc = DateTime.UtcNow;
    }

    public void Pin() => IsPinned = true;
    public void Unpin() => IsPinned = false;
}