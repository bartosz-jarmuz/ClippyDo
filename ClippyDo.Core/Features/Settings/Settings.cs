namespace ClippyDo.Core.Features.Settings;

public sealed class Settings
{
    // Limits
    public int MaxStoredItems { get; init; } = 500;
    public int SimpleCompareMaxChars { get; init; } = 4000;

    // UI
    public bool PlacePickerNearMouse { get; init; } = true;
    public double PreviewMaxScreenFraction { get; init; } = 0.5; // 50%

    // Filters (regex)
    public string NumbersRegex { get; init; } = @"^-?\d+(?:[\.,]\d+)?$";
    public string PathsRegex { get; init; } = @"^([A-Za-z]:\\|/).+";
    public string? CustomRegex { get; init; }

    // Hotkeys (string form e.g. "Ctrl+Shift+V")
    public string HotkeyPicker { get; init; } = "Ctrl+Shift+V";
    public string HotkeyCompare { get; init; } = "Ctrl+Shift+C";
    public string HotkeyPastePlain { get; init; } = "Shift+Enter";

    // Persistence
    public string DatabasePath { get; init; } = "%AppData%/ClippyDo/clippydo.db";

    // Columns layout (simplified placeholder)
    public ColumnsLayout ColumnsLayout { get; init; } = ColumnsLayout.Default();
}