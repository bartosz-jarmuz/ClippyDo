using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;
using ClippyDo.Core.Features.Settings;

namespace ClippyDo.Core.Services;

public sealed class TaggingService
{
    private readonly IRegexMatcher _regex;
    private readonly Settings _settings;

    public TaggingService(IRegexMatcher regex, Settings settings)
    {
        _regex = regex; _settings = settings;
    }

    public void ApplyStandardTags(Clip clip)
    {
        if (clip.Kind == ClipKind.Text && !string.IsNullOrEmpty(clip.PlainText))
        {
            var text = clip.PlainText!;
            if (_regex.IsMatch(text, _settings.NumbersRegex)) clip.Tags.Add("IsNumber");
            if (_settings.CustomRegex is { Length: > 0 } && _regex.IsMatch(text, _settings.CustomRegex)) clip.Tags.Add("MatchesCustomRegex");
            if (_regex.IsMatch(text, _settings.PathsRegex)) clip.Tags.Add("IsPath");
        }
    }
}