using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Windows;

internal sealed class WindowsScreenBounds : IScreenBounds
{
    public (double X, double Y, double Width, double Height) GetWorkAreaNearPointer() => (0, 0, 800, 600);
}