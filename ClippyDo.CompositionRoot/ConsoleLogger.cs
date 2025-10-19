using ClippyDo.Core.Abstractions;

namespace ClippyDo.CompositionRoot;

internal sealed class ConsoleLogger : ILogger
{
    public void Error(string message, Exception? ex = null) => Console.WriteLine($"[ERR] {message} {ex}");
    public void Info(string message) => Console.WriteLine($"[INF] {message}");
    public void Warn(string message) => Console.WriteLine($"[WRN] {message}");
}