using NetArchTest.Rules;
using NUnit.Framework;

namespace ClippyDo.Tests.Architecture.TechLeakage;

[Category("Architecture")]
public class TechnologyLeakageTests
{
    private static readonly string[] WpfAndWin =
        { "System.Windows", "System.Windows.*", "Microsoft.UI", "Microsoft.Win32" };

    private static readonly string[] SqlitePkgs =
        { "Microsoft.Data.Sqlite", "System.Data.SQLite", "SQLitePCL" };

    [Test]
    public void Core_And_Infrastructure_Must_Not_Reference_WPF_Or_Win32()
    {
        foreach (var asm in new[] { AssemblyRefs.Core, AssemblyRefs.Infrastructure })
        {
            var result = Types.InAssembly(asm).Should().NotHaveDependencyOnAny(WpfAndWin).GetResult();
            Assert.That(result.IsSuccessful, Is.True,
                $"{asm.GetName().Name} leaks UI/Win32: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Test]
    public void Core_Infra_AppWpf_Must_Not_Reference_Sqlite_Packages()
    {
        foreach (var asm in new[] { AssemblyRefs.Core, AssemblyRefs.Infrastructure })
        {
            var result = Types.InAssembly(asm).Should().NotHaveDependencyOnAny(SqlitePkgs).GetResult();
            Assert.That(result.IsSuccessful, Is.True,
                $"{asm.GetName().Name} leaks Sqlite APIs: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }

        var app = AssemblyRefs.AppWpfOrNull();
        if (app != null)
        {
            var result = Types.InAssembly(app).Should().NotHaveDependencyOnAny(SqlitePkgs).GetResult();
            Assert.That(result.IsSuccessful, Is.True,
                $"App.Wpf leaks Sqlite APIs: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Test]
    public void AppWpf_Must_Not_Reference_Adapter_Assemblies()
    {
        var refs = AssemblyRefs.AppWpf.GetReferencedAssemblies().Select(a => a.Name!).ToArray();
      
        var offenders = refs.Where(x => x.Contains("ClippyDo.Adapter")).ToArray();

        Assert.That(offenders, Is.Empty,
            "ClippyDo.App.Wpf must not reference adapter assemblies. Found: " + string.Join(", ", offenders));
    }
}