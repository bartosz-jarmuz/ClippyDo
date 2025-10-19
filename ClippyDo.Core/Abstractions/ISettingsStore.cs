using ClippyDo.Core.Features.Settings;

namespace ClippyDo.Core.Abstractions;

public interface ISettingsStore
{
    Task<Settings> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(Settings settings, CancellationToken ct = default);
}