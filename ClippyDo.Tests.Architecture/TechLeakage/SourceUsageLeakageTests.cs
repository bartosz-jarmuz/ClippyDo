using NUnit.Framework;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClippyDo.Tests.Architecture.TechLeakage;

[Category("Architecture")]
public class SourceUsageLeakageTests
{
    // Any adapter usage in App (either 'using' or fully-qualified symbol)
    private static readonly Regex ForbidAdaptersInApp = new(
        @"(^\s*using\s+ClippyDo\.Adapter\.\w+\b)|(ClippyDo\.Adapter\.\w+\.)",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    // Infra: forbid adapters and WPF/Win32 imports
    private static readonly Regex ForbidBadInInfra = new(
        @"(^\s*using\s+ClippyDo\.Adapter\.\w+\b)|(ClippyDo\.Adapter\.\w+\.)|(^\s*using\s+System\.Windows\b)|(^\s*using\s+Microsoft\.Win32\b)",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    // Core: forbid WPF/Win32/SQLite imports
    private static readonly Regex ForbidBadInCore = new(
        @"(^\s*using\s+System\.Windows\b)|(^\s*using\s+Microsoft\.Win32\b)|(^\s*using\s+Microsoft\.Data\.Sqlite\b)|(^\s*using\s+System\.Data\.SQLite\b)|(^\s*using\s+SQLitePCL\b)",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    [Test]
    public void AppWpf_Source_Must_Not_Import_Adapters()
    {
        var files = SourceTree.CsFiles("ClippyDo.App.Wpf");
        var offenders = files.Where(f => ForbidAdaptersInApp.IsMatch(SourceTree.Read(f))).ToArray();

        Assert.That(offenders, Is.Empty,
            "App.Wpf files must not import adapter namespaces. Offenders:\n" +
            string.Join("\n", offenders.Select(Rel)));
    }

    [Test]
    public void Infrastructure_Source_Must_Not_Import_Adapters_Or_WPF()
    {
        var files = SourceTree.CsFiles("ClippyDo.Infrastructure");
        var offenders = files.Where(f => ForbidBadInInfra.IsMatch(SourceTree.Read(f))).ToArray();

        Assert.That(offenders, Is.Empty,
            "Infrastructure files must not import adapters or WPF/Win32. Offenders:\n" +
            string.Join("\n", offenders.Select(Rel)));
    }

    [Test]
    public void Core_Source_Must_Not_Import_WPF_Win32_Or_SQLite()
    {
        var files = SourceTree.CsFiles("ClippyDo.Core");
        var offenders = files.Where(f => ForbidBadInCore.IsMatch(SourceTree.Read(f))).ToArray();

        Assert.That(offenders, Is.Empty,
            "Core files must not import WPF/Win32/SQLite namespaces. Offenders:\n" +
            string.Join("\n", offenders.Select(Rel)));
    }

    private static string Rel(string absolutePath)
    {
        var root = SourceTree.RepoRoot().TrimEnd('\\', '/');
        return absolutePath.Replace(root, "").TrimStart('\\', '/');
    }
}
