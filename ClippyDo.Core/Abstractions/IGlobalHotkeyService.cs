namespace ClippyDo.Core.Abstractions;

public interface IGlobalHotkeyService
{
    void Register(string hotkeyId, string gesture);
    void Unregister(string hotkeyId);
    event EventHandler<string>? HotkeyPressed; // arg: hotkeyId
}