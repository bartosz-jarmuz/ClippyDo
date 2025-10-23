using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Core.Abstractions;

public interface IPasteSimulator
{
    void Paste();
    void PastePlainText(string text);
}