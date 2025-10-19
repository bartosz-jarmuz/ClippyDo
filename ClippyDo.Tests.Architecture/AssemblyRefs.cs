using System.Reflection;
// Core
using ClippyDo.Core.Features.Clipboard;
// Infrastructure
using ClippyDo.Infrastructure.Features.Clipboard;
// Adapters
using ClippyDo.Adapter.Sqlite.Registration;
using ClippyDo.Adapter.Windows.Registration;
// Composition root
using ClippyDo;

namespace ClippyDo.Tests.Architecture;

internal static class AssemblyRefs
{
    // Assemblies under test
    public static Assembly Core => typeof(Clip).Assembly;

    public static Assembly Infrastructure =>
        typeof(ClippyDo.Infrastructure.Features.Clipboard.ClipboardCapturePipeline).Assembly;

    public static Assembly AdapterSqlite => typeof(SqliteServiceCollectionExtensions).Assembly;
    public static Assembly AdapterWindows => typeof(WindowsServiceCollectionExtensions).Assembly;
    public static Assembly CompositionRoot => typeof(CompositionRoot.CompositionRoot).Assembly;

    // Root namespaces derived from representative types (robust to refactors)
    public static string CoreNsRoot => NsRootOf(typeof(Clip));

    public static string InfraNsRoot =>
        NsRootOf(typeof(ClippyDo.Infrastructure.Features.Clipboard.ClipboardCapturePipeline));

    public static string AdapterSqliteNsRoot => NsRootOf(typeof(SqliteServiceCollectionExtensions));
    public static string AdapterWindowsNsRoot => NsRootOf(typeof(WindowsServiceCollectionExtensions));
    public static string CompositionRootNsRoot => NsRootOf(typeof(CompositionRoot.CompositionRoot));

    // App.Wpf isn’t present yet
    public static Assembly? AppWpfOrNull() => null;
    public static string AppWpfNsRoot => "ClippyDo.App.Wpf"; // will be validated once the project exists

    private static string NsRootOf(Type t)
    {
        var ns = t.Namespace ?? throw new InvalidOperationException($"Type {t.FullName} has no namespace.");
        // We treat the first three segments (e.g., ClippyDo.Core, ClippyDo.Infrastructure, ClippyDo.Adapter.Sqlite)
        var parts = ns.Split('.');
        var take = Math.Min(parts.Length, 3);
        return string.Join(".", parts.Take(take));
    }
}