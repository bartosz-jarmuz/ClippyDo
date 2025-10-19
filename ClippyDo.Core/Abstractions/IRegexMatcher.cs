namespace ClippyDo.Core.Abstractions;

public interface IRegexMatcher { bool IsMatch(string input, string pattern); }