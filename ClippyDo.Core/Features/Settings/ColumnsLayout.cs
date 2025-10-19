namespace ClippyDo.Core.Features.Settings;

public sealed record ColumnsLayout(bool ShowNumbersColumn, bool ShowImagesColumn, bool ShowPathsColumn, bool ShowRegexColumn)
{
    public static ColumnsLayout Default() => new(false, false, false, false);
}