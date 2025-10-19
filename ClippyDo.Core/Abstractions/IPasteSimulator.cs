using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Core.Abstractions;

public interface IPasteSimulator
{
    void Paste(Clip clip);
    void PastePlainText(string text);
}