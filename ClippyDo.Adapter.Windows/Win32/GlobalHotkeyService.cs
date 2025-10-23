using ClippyDo.Adapter.Windows;
using ClippyDo.Adapter.Windows.Win32;
using ClippyDo.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ClippyDo.Adapter.Windows.Win32;

internal sealed class GlobalHotkeyService : IGlobalHotkeyService, IDisposable
{
    private readonly Dictionary<string, int> _ids = new();
    private readonly MessageWindow _wnd;
    private int _nextId = 1;

    public event EventHandler<string>? HotkeyPressed;

    public GlobalHotkeyService()
    {
        _wnd = new MessageWindow(HandleWndProc);
    }

    public void Register(string hotkeyId, string gesture)
    {
        if (_ids.TryGetValue(hotkeyId, out var oldId))
        {
            // Unregister on the window thread
            _wnd.Invoke(() =>
            {
                UnregisterHotKey(_wnd.Handle, oldId);
                return IntPtr.Zero;
            });
            _ids.Remove(hotkeyId);
        }

        ParseGesture(gesture, out uint mod, out uint vk);

        const uint MOD_NOREPEAT = 0x4000;
        mod |= MOD_NOREPEAT;

        int id = _nextId++;

        // Register on the window thread so the HWND/Thread affinity is perfect
        int lastError = 0;
        bool ok = false;
        _wnd.Invoke(() =>
        {
            ok = RegisterHotKey(_wnd.Handle, id, mod, vk);
            lastError = Marshal.GetLastWin32Error();
            return IntPtr.Zero;
        });

        if (!ok)
            throw new InvalidOperationException(
                $"RegisterHotKey failed for '{gesture}' (id: {hotkeyId}, modifiers: 0x{mod:X}, vk: 0x{vk:X}, Win32Error: {lastError}).");

        _ids[hotkeyId] = id;
    }

    public void Unregister(string hotkeyId)
    {
        if (_ids.TryGetValue(hotkeyId, out var id))
        {
            _wnd.Invoke(() =>
            {
                UnregisterHotKey(_wnd.Handle, id);
                return IntPtr.Zero;
            });
            _ids.Remove(hotkeyId);
        }
    }

    private (bool handled, IntPtr result) HandleWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            foreach (var kv in _ids)
                if (kv.Value == id)
                    HotkeyPressed?.Invoke(this, kv.Key);
            return (true, IntPtr.Zero);
        }
        return (false, IntPtr.Zero);
    }

    private static void ParseGesture(string gesture, out uint modifiers, out uint key)
    {
        modifiers = 0; key = 0;

        foreach (var raw in gesture.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var p = raw.ToLowerInvariant();
            switch (p)
            {
                case "ctrl": modifiers |= 0x0002; break; // MOD_CONTROL
                case "shift": modifiers |= 0x0004; break; // MOD_SHIFT
                case "alt": modifiers |= 0x0001; break; // MOD_ALT
                case "win": modifiers |= 0x0008; break; // MOD_WIN
                default:
                    key = VkFromToken(raw);
                    break;
            }
        }

        if (key == 0)
            throw new ArgumentException($"Cannot parse gesture key from '{gesture}'.");
    }

    private static uint VkFromToken(string token)
    {
        string t = token.Trim();

        if (t.Length == 1)
        {
            char ch = char.ToUpperInvariant(t[0]);
            if (char.IsLetterOrDigit(ch)) return (uint)ch;
        }

        t = t.ToUpperInvariant();

        if (t.Length >= 2 && t[0] == 'F' && int.TryParse(t.AsSpan(1), out int fn) && fn is >= 1 and <= 24)
            return (uint)(0x70 + (fn - 1)); // VK_F1 = 0x70

        return t switch
        {
            "ENTER" => 0x0D,
            "ESC" or "ESCAPE" => 0x1B,
            "TAB" => 0x09,
            "SPACE" => 0x20,
            "UP" or "UPARROW" => 0x26,
            "DOWN" or "DOWNARROW" => 0x28,
            "LEFT" or "LEFTARROW" => 0x25,
            "RIGHT" or "RIGHTARROW" => 0x27,
            "HOME" => 0x24,
            "END" => 0x23,
            "PGUP" or "PAGEUP" => 0x21,
            "PGDN" or "PAGEDOWN" => 0x22,
            "INSERT" => 0x2D,
            "DELETE" => 0x2E,
            "BACKSPACE" or "BKSP" => 0x08,
            "PRINTSCREEN" or "PRTSC" => 0x2C,
            "C" => 0x43,
            "V" => 0x56,
            "X" => 0x58,
            "Y" => 0x59,
            "Z" => 0x5A,
            _ => 0
        };
    }

    [DllImport("user32.dll", SetLastError = true)] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public void Dispose()
    {
        foreach (var id in _ids.Values)
        {
            try
            {
                _wnd.Invoke(() =>
                {
                    UnregisterHotKey(_wnd.Handle, id);
                    return IntPtr.Zero;
                });
            }
            catch { /* swallow on dispose */ }
        }
        _ids.Clear();
        _wnd.Dispose();
    }
}
