using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ClippyDo.CompositionRoot;
using ClippyDo.App.Wpf.Features.Picker;
using ClippyDo.App.Wpf.Services;
using Application = System.Windows.Application;

namespace ClippyDo.App.Wpf;

public partial class App : Application
{
    private ServiceProvider? _provider;
    private TrayIconManager? _tray;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var services = new ServiceCollection()
            .AddClippyDo(config)
            .AddSingleton<PickerViewModel>()
            .AddTransient<PickerWindow>()
            .AddSingleton<TrayIconManager>(sp =>
                new TrayIconManager(() => sp.GetRequiredService<PickerWindow>()));

        _provider = services.BuildServiceProvider();

        await _provider.RunStartupTasksAsync();

        _tray = _provider.GetRequiredService<TrayIconManager>();

        // optional: start hidden in tray; bring up on hotkey later
        // _tray.ShowMainWindow();

        // For now, show once so you can see it:
        _tray.ShowMainWindow();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _provider?.Dispose();
        base.OnExit(e);
    }
}