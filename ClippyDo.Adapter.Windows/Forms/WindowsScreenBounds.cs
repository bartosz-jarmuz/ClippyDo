using System.Drawing;
using System.Windows.Forms;
using ClippyDo.Core.Abstractions;

namespace ClippyDo.Adapter.Windows.Forms;

internal sealed class WindowsScreenBounds : IScreenBounds
{
    public (double X, double Y, double Width, double Height) GetWorkAreaNearPointer()
    {
        var pt = Cursor.Position;
        var wa = Screen.FromPoint(new Point(pt.X, pt.Y)).WorkingArea;
        return (wa.X, wa.Y, wa.Width, wa.Height);
    }
}