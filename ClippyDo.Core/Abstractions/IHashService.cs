using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.Core.Abstractions;

public interface IHashService { ContentHash Compute(Clip clip); }