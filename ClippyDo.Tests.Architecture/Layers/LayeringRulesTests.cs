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
            AssemblyRefs.AdapterSqliteNsRoot,
            AssemblyRefs.AdapterWindowsNsRoot,
            AssemblyRefs.CompositionRootNsRoot,
            AssemblyRefs.AppWpfNsRoot,
            "System.Windows", "Microsoft.UI", "Microsoft.Win32",
            "Microsoft.Data.Sqlite", "System.Data.SQLite", "SQLitePCL"
        };

        var res = Types.InAssembly(AssemblyRefs.Core)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            // Show which forbidden namespaces were matched
            var offenders = Types.InAssembly(AssemblyRefs.Core)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail(
                "Core has forbidden deps:\n" +
                string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }

    [Test]
    public void Infrastructure_Must_Depend_Only_On_Core_And_BCL()
    {
        var forbidden = new[]
        {
            AssemblyRefs.AdapterSqliteNsRoot,
            AssemblyRefs.AdapterWindowsNsRoot,
            AssemblyRefs.CompositionRootNsRoot,
            AssemblyRefs.AppWpfNsRoot,
            "System.Windows", "Microsoft.UI", "Microsoft.Win32",
            "Microsoft.Data.Sqlite", "System.Data.SQLite", "SQLitePCL"
        };

        var res = Types.InAssembly(AssemblyRefs.Infrastructure)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            var offenders = Types.InAssembly(AssemblyRefs.Infrastructure)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail(
                "Infrastructure has forbidden deps:\n" +
                string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }

    [Test]
    public void Adapters_Sqlite_Must_Depend_Only_On_Core()
    {
        var forbidden = new[]
        {
            AssemblyRefs.InfraNsRoot,
            AssemblyRefs.AdapterWindowsNsRoot,
            AssemblyRefs.CompositionRootNsRoot,
            AssemblyRefs.AppWpfNsRoot
        };

        var res = Types.InAssembly(AssemblyRefs.AdapterSqlite)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            var offenders = Types.InAssembly(AssemblyRefs.AdapterSqlite)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail(
                "Adapter.Sqlite has forbidden deps:\n" +
                string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }

    [Test]
    public void Adapters_Windows_Must_Depend_Only_On_Core()
    {
        var forbidden = new[]
        {
            AssemblyRefs.InfraNsRoot,
            AssemblyRefs.AdapterSqliteNsRoot,
            AssemblyRefs.CompositionRootNsRoot,
            AssemblyRefs.AppWpfNsRoot
        };

        var res = Types.InAssembly(AssemblyRefs.AdapterWindows)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            var offenders = Types.InAssembly(AssemblyRefs.AdapterWindows)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail(
                "Adapter.Windows has forbidden deps:\n" +
                string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }

    [Test]
    public void CompositionRoot_May_Not_Depend_On_AppWpf()
    {
        var res = Types.InAssembly(AssemblyRefs.CompositionRoot)
            .Should().NotHaveDependencyOnAny(AssemblyRefs.AppWpfNsRoot)
            .GetResult();

        Assert.That(res.IsSuccessful, Is.True,
            () => "CompositionRoot must not depend on App.Wpf: " + string.Join("\n", res.FailingTypeNames));
    }

    [Test]
    public void AppWpf_When_Present_Must_Only_Depend_On_Core_And_CompositionRoot()
    {
        var app = AssemblyRefs.AppWpfOrNull();
        if (app is null) Assert.Pass("App.Wpf not present yet.");

        var forbidden = new[]
        {
            AssemblyRefs.InfraNsRoot,
            AssemblyRefs.AdapterSqliteNsRoot,
            AssemblyRefs.AdapterWindowsNsRoot
        };

        var res = Types.InAssembly(app!)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        if (!res.IsSuccessful)
        {
            var offenders = Types.InAssembly(app!)
                .That().HaveDependencyOnAny(forbidden)
                .GetTypes();

            Assert.Fail(
                "App.Wpf has forbidden deps:\n" +
                string.Join("\n", offenders.Select(t => $" - {t.FullName}")));
        }
    }
}
