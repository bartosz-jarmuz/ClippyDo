using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Windows;

internal sealed class HotkeysRegistrationStartupTask : IStartupTask
{
    private readonly IGlobalHotkeyService _hotkeys;
    private readonly ISettingsStore _settings;

    public HotkeysRegistrationStartupTask(IGlobalHotkeyService hotkeys, ISettingsStore settings)
    { _hotkeys = hotkeys; _settings = settings; }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var s = await _settings.LoadAsync(ct);
        _hotkeys.Register("Picker", s.HotkeyPicker);
        _hotkeys.Register("Compare", s.HotkeyCompare);
    }
}