namespace ClippyDo.Tests.Architecture.TechLeakage;

[Category("Architecture")]
public class AssemblyReferenceLeakageTests
{
    [Test]
    public void AppWpf_Must_Not_Reference_Adapter_Assemblies()
    {
        var refs = AssemblyRefs.AppWpf.GetReferencedAssemblies().Select(a => a.Name!).ToArray();
        var offenders = refs.Where(n => n.StartsWith("ClippyDo.Adapter")).ToArray();

        Assert.That(offenders, Is.Empty,
            "ClippyDo.App.Wpf must not reference adapter assemblies. Found: " + string.Join(", ", offenders));
    }

    [Test]
    public void Core_Must_Not_Reference_Adapter_Assemblies()
    {
        var refs = AssemblyRefs.Core.GetReferencedAssemblies().Select(a => a.Name!).ToArray();
        var offenders = refs.Where(n => n.StartsWith("ClippyDo.Adapter")).ToArray();

        Assert.That(offenders, Is.Empty,
            "ClippyDo.Core must not reference adapter assemblies. Found: " + string.Join(", ", offenders));
    }

    [Test]
    public void Infrastructure_Must_Not_Reference_Adapter_Assemblies()
    {
        var refs = AssemblyRefs.Infrastructure.GetReferencedAssemblies().Select(a => a.Name!).ToArray();
        var offenders = refs.Where(n => n.StartsWith("ClippyDo.Adapter")).ToArray();

        Assert.That(offenders, Is.Empty,
            "ClippyDo.Infrastructure must not reference adapter assemblies. Found: " + string.Join(", ", offenders));
    }

    [Test]
    public void CompositionRoot_Must_Reference_At_Least_One_Adapter_Assembly()
    {
        var refs = AssemblyRefs.CompositionRoot.GetReferencedAssemblies().Select(a => a.Name!).ToArray();
        var offenders = refs.Where(n => n.StartsWith("ClippyDo.Adapter")).ToArray();

        Assert.That(offenders.Length, Is.GreaterThanOrEqualTo(1),
            "CompositionRoot is expected to reference one or more adapter assemblies.");
    }
}