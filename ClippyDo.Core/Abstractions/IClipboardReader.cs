using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Core.Abstractions;

public interface IClipboardReader
{
    // Reads all available formats; callers can choose what to persist
    Clip ReadCurrent();
}