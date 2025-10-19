using ClippyDo.Core.Abstractions;

namespace ClippyDo.CompositionRoot;

internal sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }