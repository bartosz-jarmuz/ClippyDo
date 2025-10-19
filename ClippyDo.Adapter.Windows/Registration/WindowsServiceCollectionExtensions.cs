using Microsoft.Extensions.DependencyInjection;
using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Windows.Registration;

public static class WindowsServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsAdapters(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
        services.AddSingleton<IClipboardMonitor, WindowsClipboardMonitor>();
        services.AddSingleton<IClipboardReader, WindowsClipboardReader>();
        services.AddSingleton<IPasteSimulator, WindowsPasteSimulator>();
        services.AddSingleton<IScreenBounds, WindowsScreenBounds>();
        services.AddSingleton<IStartupTask, HotkeysRegistrationStartupTask>();
        return services;
    }
}