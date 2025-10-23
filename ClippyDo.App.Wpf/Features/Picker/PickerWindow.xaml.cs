using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ClippyDo.App.Wpf.Services;

namespace ClippyDo.App.Wpf.Features.Picker;

public partial class PickerWindow : Window
{
    private readonly TrayIconManager _tray;

    // CHANGED: accept TrayIconManager via DI
    public PickerWindow(TrayIconManager tray)
    {
        InitializeComponent();
        _tray = tray;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;   // close always hides to tray
        HideToTray();
    }

    private void HideToTray() => Hide();
}