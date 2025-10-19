using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace ClippyDo.Tests.Architecture.Layers;

[Category("Architecture")]
public class PortsAdaptersTests
{
    private static readonly Assembly Core = AssemblyRefs.Core;
    private static readonly Assembly Infra = AssemblyRefs.Infrastructure;
    private static readonly Assembly Sqlite = AssemblyRefs.AdapterSqlite;
    private static readonly Assembly Windows = AssemblyRefs.AdapterWindows;

    [Test]
    public void All_Public_Interfaces_Used_By_Adapters_Or_Infrastructure_Are_Defined_In_Core()
    {
        static bool IsFrameworkNs(string ns) =>
            ns.StartsWith("System", StringComparison.Ordinal) ||
            ns.StartsWith("Microsoft", StringComparison.Ordinal);

        var coreInterfaces = Core.GetTypes().Where(t => t.IsInterface && t.IsPublic).ToHashSet();

        bool InterfaceOwnedByCore(Type itf) =>
            IsFrameworkNs(itf.Namespace ?? "") || coreInterfaces.Contains(itf);

        foreach (var asm in new[] { Sqlite, Windows, Infra })
        {
            var offenders = asm.GetTypes()
                .Where(t => t.IsClass)
                .SelectMany(t => t.GetInterfaces())
                .Where(i => i.IsPublic && !InterfaceOwnedByCore(i))
                .Distinct()
                .ToArray();

            Assert.That(offenders, Is.Empty,
                $"{asm.GetName().Name}: These public interfaces are not owned by Core: {string.Join(", ", offenders.Select(o => o.FullName))}");
        }
    }

    [Test]
    public void Adapters_Should_Not_Declare_Public_Interfaces()
    {
        foreach (var asm in new[] { Sqlite, Windows })
        {
            var offenders = asm.GetTypes()
                .Where(t => t.IsInterface && t.IsPublic)
                .Select(t => t.FullName)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                $"{asm.GetName().Name}: Adapters must not expose public interfaces. Found: {string.Join(", ", offenders)}");
        }
    }

    [Test]
    public void No_Public_API_Leakage_Of_Adapter_Types_In_Core_Or_Infrastructure()
    {
        static bool IsAdapterNs(string? ns) =>
            (ns != null) && (ns.StartsWith(AssemblyRefs.AdapterSqliteNsRoot) || ns.StartsWith(AssemblyRefs.AdapterWindowsNsRoot));

        foreach (var asm in new[] { Core, Infra })
        {
            var offenders =
                asm.GetExportedTypes()
                   .SelectMany(t => t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                   .SelectMany(m => m switch
                   {
                       MethodInfo mi => new[] { mi.ReturnType }.Concat(mi.GetParameters().Select(p => p.ParameterType)),
                       PropertyInfo pi => new[] { pi.PropertyType },
                       FieldInfo fi => new[] { fi.FieldType },
                       EventInfo ei => new[] { ei.EventHandlerType! },
                       _ => Array.Empty<Type>()
                   })
                   .Where(t => IsAdapterNs(t.Namespace))
                   .Distinct()
                   .ToArray();

            Assert.That(offenders, Is.Empty,
                $"{asm.GetName().Name}: Public API must not expose adapter types: {string.Join(", ", offenders.Select(o => o.FullName))}");
        }
    }
}
