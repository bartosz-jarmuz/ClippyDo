using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Windows;

internal sealed class GlobalHotkeyService : IGlobalHotkeyService
{
    public event EventHandler<string>? HotkeyPressed;
    public void Register(string hotkeyId, string gesture) { /* TODO */ }
    public void Unregister(string hotkeyId) { /* TODO */ }
}