using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Windows.Registration;

public static class WindowsServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsAdapters(this IServiceCollection services)
        => services
            .AddWindowsMessaging()
            .AddWindowsClipboard()
            .AddWindowsDisplay()
            .AddWindowsStartupTasks();
}