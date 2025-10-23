using ClippyDo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClippyDo.Adapter.Windows.Registration;

public static class WindowsStartupServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsStartupTasks(this IServiceCollection services)
    {
        services.AddSingleton<IStartupTask, HotkeysRegistrationStartupTask>();
        return services;
    }
}