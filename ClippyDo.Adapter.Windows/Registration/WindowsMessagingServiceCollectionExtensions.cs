using ClippyDo.Adapter.Windows.Win32;
using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Windows.Registration;

public static class WindowsMessagingServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IClipboardMonitor, WindowsClipboardMonitor>();
        services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
        return services;
    }
}