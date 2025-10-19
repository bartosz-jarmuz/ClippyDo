namespace ClippyDo.Core.Features.Clipboard;

public readonly record struct ContentHash(string Value)
{
    public override string ToString() => Value;
}