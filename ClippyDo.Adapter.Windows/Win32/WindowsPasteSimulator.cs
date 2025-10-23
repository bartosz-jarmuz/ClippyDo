using System;
using System.Runtime.InteropServices;
using System.Windows;
using ClippyDo.Adapter.Windows.Interop;
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
        System.Windows.IDataObject? backup = null;

        try
        {
            // Snapshot current clipboard (best-effort)
            backup = ClipboardRetry.Run(() => Clipboard.GetDataObject());

            // Put plain text on clipboard with "copy=true" to avoid owning it longer than needed
            ClipboardRetry.Run(() =>
            {
                Clipboard.SetDataObject(text ?? string.Empty, /* copy: */ true);
                return 0;
            });

            // Do the paste (target app reads the current CF_UNICODETEXT)
            Paste();

            // Restore previous clipboard shortly after paste, not synchronously
            _ = Task.Run(async () =>
            {
                await Task.Delay(150); // small delay to let target app read
                if (backup is not null)
                {
                    try
                    {
                        ClipboardRetry.Run(() =>
                        {
                            Clipboard.SetDataObject(backup, /* copy: */ true);
                            return 0;
                        }, attempts: 8, initialDelayMs: 16);
                    }
                    catch
                    {
                        // Swallow restore failures; user won't want an error here
                    }
                }
            });
        }
        catch
        {
            // Swallow: plain paste must be best-effort and never throw
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
