using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Adapter.Windows;

internal sealed class WindowsClipboardReader : IClipboardReader
{
    public Clip ReadCurrent() => new() { Kind = ClipKind.Text, PlainText = "", ContentHash = new("") };
}