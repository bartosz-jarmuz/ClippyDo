namespace ClippyDo.Core.Features.Clipboard;

public readonly record struct ClipId(string Value)
{
    public override string ToString() => Value;
    public static ClipId New() => new(Guid.NewGuid().ToString("N"));
}