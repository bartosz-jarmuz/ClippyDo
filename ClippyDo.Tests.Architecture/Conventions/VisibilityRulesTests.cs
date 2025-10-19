using NUnit.Framework;
using System.Linq;

namespace ClippyDo.Tests.Architecture.Conventions;

[Category("Architecture")]
public class VisibilityRulesTests
{
    [Test]
    public void Adapter_Sqlite_Public_Types_Must_Be_DI_Installers_Only()
    {
        var asm = AssemblyRefs.AdapterSqlite;
        var offenders = asm.GetExportedTypes()
            .Where(t =>
                !(t.IsAbstract && t.IsSealed && // static class
                  t.Name.EndsWith("ServiceCollectionExtensions") &&
                  (t.Namespace?.Contains(".Registration") ?? false)))
            .Select(t => t.FullName)
            .ToArray();

        Assert.That(offenders, Is.Empty,
            $"Adapter.Sqlite should only expose DI installers as public types. Offenders: {string.Join(", ", offenders)}");
    }

    [Test]
    public void Adapter_Windows_Public_Types_Must_Be_DI_Installers_Only()
    {
        var asm = AssemblyRefs.AdapterWindows;
        var offenders = asm.GetExportedTypes()
            .Where(t =>
                !(t.IsAbstract && t.IsSealed && // static class
                  t.Name.EndsWith("ServiceCollectionExtensions") &&
                  (t.Namespace?.Contains(".Registration") ?? false)))
            .Select(t => t.FullName)
            .ToArray();

        Assert.That(offenders, Is.Empty,
            $"Adapter.Windows should only expose DI installers as public types. Offenders: {string.Join(", ", offenders)}");
    }
}
