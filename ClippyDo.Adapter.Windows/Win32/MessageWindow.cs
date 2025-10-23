using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ClippyDo.Adapter.Windows.Win32;

/// <summary>
/// Hidden top-level Win32 window hosted on its own STA thread with a message loop.
/// Provides a marshal mechanism (Invoke/InvokeAsync) to run code on the window thread,
/// which is useful for APIs that require same-thread window ownership (e.g., RegisterHotKey).
/// </summary>
internal sealed class MessageWindow : IDisposable
{
    private readonly Thread _thread;
    private readonly AutoResetEvent _ready = new(false);
    private readonly Func<IntPtr, int, IntPtr, IntPtr, (bool handled, IntPtr result)> _handler;

    private IntPtr _hwnd;
    private WndProcDelegate? _wndProcThunk; // keep alive for the lifetime of the window
    private string? _className;
    private bool _disposed;

    // Simple single-slot action marshalling (sufficient for our usage)
    private TaskCompletionSource<IntPtr>? _pendingTcs;
    private Func<IntPtr>? _pendingAction;

    public IntPtr Handle => _hwnd;

    public MessageWindow(Func<IntPtr, int, IntPtr, IntPtr, (bool handled, IntPtr result)> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));

        _thread = new Thread(ThreadMain)
        {
            Name = "ClippyDo.MessageWindow",
            IsBackground = true
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();

        // wait until HWND is ready or an exception occurs
        _ready.WaitOne();
        if (_hwnd == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create hidden message window.");
    }

    private void ThreadMain()
    {
        try
        {
            _className = "ClippyDo_HiddenTopLevel_" + Guid.NewGuid().ToString("N");
            _wndProcThunk = WndProc;

            var hInstance = GetModuleHandle(null);
            if (hInstance == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "GetModuleHandle returned NULL.");

            var wcx = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                style = 0,
                lpfnWndProc = _wndProcThunk,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = hInstance,
                hIcon = IntPtr.Zero,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = _className,
                hIconSm = IntPtr.Zero
            };

            ushort atom = RegisterClassEx(ref wcx);
            if (atom == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "RegisterClassEx failed for MessageWindow.");

            // Hidden top-level window
            _hwnd = CreateWindowEx(
                WS_EX_TOOLWINDOW,
                _className,
                "ClippyDoHidden",
                WS_POPUP,
                0, 0, 0, 0,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "CreateWindowEx failed for MessageWindow.");

            ShowWindow(_hwnd, SW_HIDE);

            _ready.Set();

            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
        catch
        {
            _ready.Set(); // unblock ctor on failure
            throw;
        }
    }

    private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        const int WM_DESTROY = 0x0002;

        if (msg == WM_APP_INVOKE)
        {
            // Run pending action on this (window) thread
            var tcs = _pendingTcs;
            var action = _pendingAction;
            _pendingTcs = null;
            _pendingAction = null;

            if (tcs is not null && action is not null)
            {
                try
                {
                    IntPtr r = action.Invoke();
                    tcs.TrySetResult(r);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
            return IntPtr.Zero;
        }

        var (handled, result) = _handler(hWnd, msg, wParam, lParam);
        if (handled) return result;

        if (msg == WM_DESTROY)
        {
            PostQuitMessage(0);
            return IntPtr.Zero;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    /// <summary>
    /// Marshal an action onto the window thread and wait synchronously for completion.
    /// The action returns an IntPtr (can be unused).
    /// </summary>
    public void Invoke(Func<IntPtr> action)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(MessageWindow));
        var tcs = new TaskCompletionSource<IntPtr>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingTcs = tcs;
        _pendingAction = action;
        if (!PostMessage(_hwnd, WM_APP_INVOKE, IntPtr.Zero, IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "PostMessage failed for Invoke.");
        tcs.Task.GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_hwnd != IntPtr.Zero)
            {
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }

            if (!string.IsNullOrEmpty(_className))
            {
                var hInstance = GetModuleHandle(null);
                UnregisterClass(_className, hInstance);
                _className = null;
            }
        }
        catch
        {
            // Swallow on dispose
        }
        finally
        {
            _ready.Dispose();
        }
    }

    // =========================
    // Win32 interop
    // =========================

    private const int WS_POPUP = unchecked((int)0x80000000);
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int SW_HIDE = 0;

    private const int WM_APP = 0x8000;
    private const int WM_APP_INVOKE = WM_APP + 1;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        public WndProcDelegate? lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string? lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int X, int Y, int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
    [DllImport("user32.dll")] private static extern bool TranslateMessage([In] ref MSG lpMsg);
    [DllImport("user32.dll")] private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);
    [DllImport("user32.dll")] private static extern IntPtr DefWindowProc(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool DestroyWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern void PostQuitMessage(int nExitCode);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
}
