using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using ClippyDo.Adapter.Windows.Interop;
using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using TextDataFormat = System.Windows.TextDataFormat;

namespace ClippyDo.Adapter.Windows.Wpf;

internal sealed class WindowsClipboardReader : IClipboardReader
{
    public Clip ReadCurrent() => ClipboardRetry.Run(() =>
    {
        if (Clipboard.ContainsImage())
        {
            var bmp = Clipboard.GetImage();
            if (bmp != null)
            {
                using var ms = new System.IO.MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(ms);

                return new Clip
                {
                    Kind = ClipKind.Image,
                    ImageBytes = ms.ToArray(),
                    ImageFormat = "png",
                    ThumbnailBytes = ms.ToArray(), // TODO: generate smaller thumbnail off-UI
                    Source = GetForegroundSource()
                };
            }
        }

        if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
        {
            var text = Clipboard.GetText(TextDataFormat.UnicodeText);
            byte[]? rtf = Clipboard.ContainsData(DataFormats.Rtf)
                ? Clipboard.GetData(DataFormats.Rtf) as string is { } r ? Encoding.UTF8.GetBytes(r) : null
                : null;

            byte[]? html = Clipboard.ContainsData(DataFormats.Html)
                ? Clipboard.GetData(DataFormats.Html) as string is { } h ? Encoding.UTF8.GetBytes(h) : null
                : null;

            return new Clip
            {
                Kind = ClipKind.Text,
                PlainText = text,
                Rtf = rtf,
                Html = html,
                Source = GetForegroundSource()
            };
        }

        // Fallback empty text clip (rare non-text/image formats are ignored by design)
        return new Clip { Kind = ClipKind.Text, PlainText = string.Empty, Source = GetForegroundSource() };
    });

    private static SourceApp GetForegroundSource()
    {
        var h = GetForegroundWindow();
        GetWindowThreadProcessId(h, out var pid);
        try
        {
            var p = Process.GetProcessById((int)pid);
            var title = GetWindowTitle(h);
            return new SourceApp(p.ProcessName, p.MainModule?.FileName ?? p.ProcessName, title);
        }
        catch
        {
            return new SourceApp("Unknown", "Unknown", null);
        }
    }

    private static string? GetWindowTitle(nint hWnd)
    {
        var sb = new StringBuilder(1024);
        _ = GetWindowText(hWnd, sb, sb.Capacity);
        var s = sb.ToString();
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll", SetLastError = true)] private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);
}
