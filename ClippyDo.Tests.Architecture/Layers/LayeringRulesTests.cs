using NetArchTest.Rules;
using NUnit.Framework;
using System.Linq;

namespace ClippyDo.Tests.Architecture.Layers;

[Category("Architecture")]
public class LayeringRulesTests
{
    [Test]
    public void Core_Must_Not_Depend_On_UI_Adapters_Infra_Or_Sqlite()
    {
        var forbidden = new[]
        {
            AssemblyRefs.InfraNsRoot,
            AssemblyRefs.CompositionRootNsRoot,
            AssemblyRefs.AppWpfNsRoot,
            "System.Windows", "Microsoft.UI", "Microsoft.Win32",
            "Microsoft.Data.Sqlite", "System.Data.SQLite", "SQLitePCL"
        }.Concat(AssemblyRefs.AdapterNsRoots()).ToArray();

        var res = Types.InAssembly(AssemblyRefs.Core)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            var offenders = Types.InAssembly(AssemblyRefs.Core)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail("Core has forbidden deps:\n" +
                        string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }

    [Test]
    public void Infrastructure_Must_Depend_Only_On_Core_And_BCL()
    {
        var forbidden = new[]
        {
            AssemblyRefs.CompositionRootNsRoot,
            AssemblyRefs.AppWpfNsRoot,
            "System.Windows", "Microsoft.UI", "Microsoft.Win32",
            "Microsoft.Data.Sqlite", "System.Data.SQLite", "SQLitePCL"
        }.Concat(AssemblyRefs.AdapterNsRoots()).ToArray();

        var res = Types.InAssembly(AssemblyRefs.Infrastructure)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            var offenders = Types.InAssembly(AssemblyRefs.Infrastructure)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail("Infrastructure has forbidden deps:\n" +
                        string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }

    [Test]
    public void Adapters_Must_Depend_Only_On_Core()
    {
        var forbidden = new[]
        {
            AssemblyRefs.InfraNsRoot,
            AssemblyRefs.CompositionRootNsRoot,
            AssemblyRefs.AppWpfNsRoot
        };

        foreach (var asm in AssemblyRefs.AdapterAssemblies())
        {
            var res = Types.InAssembly(asm)
                .Should().NotHaveDependencyOnAny(forbidden)
                .GetResult();

            if (!res.IsSuccessful)
            {
                var offenders = Types.InAssembly(asm)
                    .That().HaveDependencyOnAny(forbidden)
                    .GetTypes();

                Assert.Fail($"{asm.GetName().Name} has forbidden deps:\n" +
                            string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
            }
        }
    }

    [Test]
    public void CompositionRoot_May_Not_Depend_On_AppWpf()
    {
        var res = Types.InAssembly(AssemblyRefs.CompositionRoot)
            .Should().NotHaveDependencyOnAny(AssemblyRefs.AppWpfNsRoot)
            .GetResult();

        Assert.That(res.IsSuccessful, Is.True,
            () => "CompositionRoot must not depend on App.Wpf:\n" + string.Join("\n", res.FailingTypeNames));
    }

    [Test]
    public void AppWpf_Must_Only_Depend_On_Core_And_CompositionRoot()
    {
        var app = AssemblyRefs.AppWpfOrNull();
        if (app is null) Assert.Pass("App.Wpf not present yet.");

        var forbidden = new[]
        {
            AssemblyRefs.InfraNsRoot
        }.Concat(AssemblyRefs.AdapterNsRoots()).ToArray();

        var res = Types.InAssembly(app!)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            var offenders = Types.InAssembly(app!)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail("App.Wpf has forbidden deps:\n" +
                        string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }
}
