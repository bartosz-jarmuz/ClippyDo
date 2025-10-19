using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.CompositionRoot;

internal sealed class DefaultHashService : IHashService
{
    public ContentHash Compute(Clip clip)
    {
        // Simple placeholder hash – replace with stable content-kind-aware hashing
        var bytes = clip.Kind == ClipKind.Text ? System.Text.Encoding.UTF8.GetBytes(clip.PlainText ?? string.Empty) : (clip.ImageBytes ?? Array.Empty<byte>());
        using var sha = System.Security.Cryptography.SHA256.Create();
        return new ContentHash(Convert.ToHexString(sha.ComputeHash(bytes)));
    }
}