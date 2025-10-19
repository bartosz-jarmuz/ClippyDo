using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Windows;

internal sealed class WindowsClipboardMonitor : IClipboardMonitor
{
    public event EventHandler? ClipboardChanged;
    public void Start() { /* TODO */ }
    public void Stop() { /* TODO */ }
}