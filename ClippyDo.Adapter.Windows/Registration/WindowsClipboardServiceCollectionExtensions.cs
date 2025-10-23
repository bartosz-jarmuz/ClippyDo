using ClippyDo.Adapter.Windows.Win32;
using ClippyDo.Adapter.Windows.Wpf;
using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Windows.Registration;

public static class WindowsClipboardServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsClipboard(this IServiceCollection services)
    {
        services.AddSingleton<IClipboardReader, WindowsClipboardReader>(); // WPF Clipboard (+ retry)
        services.AddSingleton<IPasteSimulator, WindowsPasteSimulator>();   // Win32 SendInput
        return services;
    }
}