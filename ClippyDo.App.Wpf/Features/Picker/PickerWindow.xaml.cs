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

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        else
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HideToTray();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;   // close always hides to tray
        HideToTray();
    }

    private void HideToTray() => Hide();
}