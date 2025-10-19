// Clipboard & Hotkeys

namespace ClippyDo.Core.Abstractions;

public interface IClipboardMonitor
{
    event EventHandler? ClipboardChanged;
    void Start();
    void Stop();
}

// Persistence & Search

// System & Utilities