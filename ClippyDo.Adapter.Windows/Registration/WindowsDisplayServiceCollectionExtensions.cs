using ClippyDo.Adapter.Windows.Forms;
using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Windows.Registration;

public static class WindowsDisplayServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsDisplay(this IServiceCollection services)
    {
        services.AddSingleton<IScreenBounds, WindowsScreenBounds>(); // WinForms Screen.FromPoint
        return services;
    }
}