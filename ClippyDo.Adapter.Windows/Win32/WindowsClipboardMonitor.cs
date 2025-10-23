using System;
using System.Runtime.InteropServices;
using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Windows.Win32;

internal sealed class WindowsClipboardMonitor : IClipboardMonitor, IDisposable
{
    private MessageWindow? _wnd;

    public event EventHandler? ClipboardChanged;

    public void Start()
    {
        if (_wnd is not null) return;
        _wnd = new MessageWindow(HandleWndProc);
        if (!AddClipboardFormatListener(_wnd.Handle))
            throw new InvalidOperationException("AddClipboardFormatListener failed.");
    }

    public void Stop()
    {
        if (_wnd is null) return;
        RemoveClipboardFormatListener(_wnd.Handle);
        _wnd.Dispose();
        _wnd = null;
    }

    private (bool handled, nint result) HandleWndProc(nint hwnd, int msg, nint wParam, nint lParam)
    {
        const int WM_CLIPBOARDUPDATE = 0x031D;
        if (msg == WM_CLIPBOARDUPDATE)
        {
            ClipboardChanged?.Invoke(this, EventArgs.Empty);
            return (true, nint.Zero);
        }
        return (false, nint.Zero);
    }

    [DllImport("user32.dll", SetLastError = true)] private static extern bool AddClipboardFormatListener(nint hwnd);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool RemoveClipboardFormatListener(nint hwnd);

    public void Dispose() => Stop();
}