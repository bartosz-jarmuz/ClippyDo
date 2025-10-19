namespace ClippyDo.Core.Abstractions;

public interface IStartupTask
{
    Task RunAsync(CancellationToken ct = default);
}