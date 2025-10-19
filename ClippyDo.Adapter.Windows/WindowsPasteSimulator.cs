using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Adapter.Windows;

internal sealed class WindowsPasteSimulator : IPasteSimulator
{
    public void Paste(Clip clip) { /* TODO */ }
    public void PastePlainText(string text) { /* TODO */ }
}