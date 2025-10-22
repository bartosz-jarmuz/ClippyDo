using System.Windows;

namespace ClippyDo.App.Wpf.Services;

public sealed class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _ni;
    private readonly Func<Window> _getMainWindow;

    public TrayIconManager(Func<Window> getMainWindow)
    {
        _getMainWindow = getMainWindow;

        _ni = new NotifyIcon
        {
            Text = "ClippyDo",
            Icon = SystemIcons.Application, // replace with your .ico when available
            Visible = true
        };

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("Open", null, (_, __) => ShowMainWindow());
        ctx.Items.Add(new ToolStripSeparator());
        ctx.Items.Add("Exit", null, (_, __) => ExitApp());
        _ni.ContextMenuStrip = ctx;

        _ni.DoubleClick += (_, __) => ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        var w = _getMainWindow();
        if (w.WindowState == WindowState.Minimized) w.WindowState = WindowState.Normal;
        w.Show();
        w.Activate();
    }

    public void HideMainWindow()
    {
        var w = _getMainWindow();
        w.Hide();
    }

    private static void ExitApp()
    {
        // ensure proper teardown
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        _ni.Visible = false;
        _ni.Dispose();
    }
}
