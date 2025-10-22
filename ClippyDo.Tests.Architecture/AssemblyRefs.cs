// Composition root
using ClippyDo;
// Adapters
using ClippyDo.Adapter.Sqlite.Registration;
using ClippyDo.Adapter.Windows.Registration;
using ClippyDo.App.Wpf.Features.Picker;
// Core
using ClippyDo.Core.Features.Clipboard;
// Infrastructure
using ClippyDo.Infrastructure.Features.Clipboard;
using System.Reflection;

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

    public static Assembly AppWpf => typeof(PickerWindow).Assembly;
    public static string AppWpfNsRoot => NsRootOf(typeof(PickerWindow));
    public static Assembly? AppWpfOrNull() => AppWpf;

    private static string NsRootOf(Type t)
    {
        var ns = t.Namespace ?? throw new InvalidOperationException($"Type {t.FullName} has no namespace.");
        // We treat the first three segments (e.g., ClippyDo.Core, ClippyDo.Infrastructure, ClippyDo.Adapter.Sqlite)
        var parts = ns.Split('.');
        var take = Math.Min(parts.Length, 3);
        return string.Join(".", parts.Take(take));
    }

    /// <summary>All currently loaded adapter assemblies whose simple name starts with "ClippyDo.Adapter".</summary>
    public static Assembly[] AdapterAssemblies() =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("ClippyDo.Adapter", StringComparison.Ordinal) == true)
            .Distinct()
            .ToArray();

    /// <summary>All adapter root namespaces derived from types in each adapter assembly.</summary>
    public static string[] AdapterNsRoots() =>
        AdapterAssemblies()
            .Select(a =>
            {
                // Pick any public type to derive namespace root (3 segments)
                var t = a.GetExportedTypes().FirstOrDefault() ?? a.GetTypes().First();
                var ns = t.Namespace ?? a.GetName().Name!;
                var parts = ns.Split('.');
                return string.Join(".", parts.Take(Math.Min(parts.Length, 3)));
            })
            .Distinct()
            .ToArray();
}