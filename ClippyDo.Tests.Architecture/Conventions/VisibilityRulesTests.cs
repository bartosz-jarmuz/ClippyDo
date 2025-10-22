using NUnit.Framework;
using System.Linq;

namespace ClippyDo.Tests.Architecture.Conventions;

[Category("Architecture")]
public class VisibilityRulesTests
{
    [Test]
    public void Adapter_Public_Types_Must_Be_DI_Installers_Only()
    {
        foreach (var asm in AssemblyRefs.AdapterAssemblies())
        {
            var offenders = asm.GetExportedTypes()
                .Where(t =>
                    !(t.IsAbstract && t.IsSealed &&
                      t.Name.EndsWith("ServiceCollectionExtensions") &&
                      (t.Namespace?.Contains(".Registration") ?? false)))
                .Select(t => $"{asm.GetName().Name}:{t.FullName}")
                .ToArray();

            Assert.That(offenders, Is.Empty,
                $"{asm.GetName().Name}: adapters should only expose DI installers as public types. Offenders:\n" +
                string.Join("\n", offenders));
        }
    }
}
