using ClippyDo.Core.Abstractions;

namespace ClippyDo.CompositionRoot;

internal sealed class SystemRegexMatcher : IRegexMatcher { public bool IsMatch(string input, string pattern) => System.Text.RegularExpressions.Regex.IsMatch(input, pattern); }