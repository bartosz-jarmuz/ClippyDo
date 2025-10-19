using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Services;

namespace ClippyDo.Infrastructure.Features.Clipboard;

public sealed class ClipboardCapturePipeline : IDisposable
{
    private readonly IClipboardMonitor _monitor;
    private readonly IClipboardReader _reader;
    private readonly IClipRepository _repo;
    private readonly IHashService _hash;
    private readonly TaggingService _tagger;

    public ClipboardCapturePipeline(
        IClipboardMonitor monitor,
        IClipboardReader reader,
        IClipRepository repo,
        IHashService hash,
        TaggingService tagger)
    {
        _monitor = monitor; _reader = reader; _repo = repo; _hash = hash; _tagger = tagger;
        _monitor.ClipboardChanged += OnClipboardChanged;
    }

    private async void OnClipboardChanged(object? sender, EventArgs e)
    {
        var clip = _reader.ReadCurrent();
        // Normalize + hash + tag
        var h = _hash.Compute(clip);
        clip.ContentHash = h; // CHANGED: no 'with' — assign to settable property
        _tagger.ApplyStandardTags(clip);
        await _repo.UpsertAsync(clip);
    }

    public void Start() => _monitor.Start();
    public void Stop() => _monitor.Stop();

    public void Dispose() => _monitor.ClipboardChanged -= OnClipboardChanged;
}