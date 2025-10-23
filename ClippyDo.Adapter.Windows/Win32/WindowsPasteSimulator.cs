using System;
using System.Runtime.InteropServices;
using System.Windows;
using ClippyDo.Core.Abstractions;
using Clipboard = System.Windows.Clipboard;
using TextDataFormat = System.Windows.TextDataFormat;

namespace ClippyDo.Adapter.Windows.Win32;

internal sealed class WindowsPasteSimulator : IPasteSimulator
{
    public void Paste()
    {
        KeyDown(VK_CONTROL);
        Tap(VK_V);
        KeyUp(VK_CONTROL);
    }

    public void PastePlainText(string text)
    {
        var backup = Clipboard.GetDataObject();
        try
        {
            Clipboard.Clear();
            Clipboard.SetText(text ?? string.Empty, TextDataFormat.UnicodeText);
            Paste();
        }
        finally
        {
            if (backup is not null)
                Clipboard.SetDataObject(backup, true);
        }
    }

    // SendInput helpers

    private static void Tap(ushort key) { KeyDown(key); KeyUp(key); }
    private static void KeyDown(ushort key) => Send(key, 0);
    private static void KeyUp(ushort key) => Send(key, KEYEVENTF_KEYUP);

    private static void Send(ushort vk, uint flags)
    {
        var input = new INPUT
        {
            type = 1, // INPUT_KEYBOARD
            U = new InputUnion
            {
                ki = new KEYBDINPUT { wVk = vk, dwFlags = flags }
            }
        };
        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
    }

    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_V = 0x56;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT { public uint type; public InputUnion U; }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public nint dwExtraInfo; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}
