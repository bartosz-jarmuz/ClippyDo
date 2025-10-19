using NUnit.Framework;
using System.Linq;

namespace ClippyDo.Tests.Architecture.Conventions;

[Category("Architecture")]
public class NamingRulesTests
{
    [Test]
    public void Core_Ports_Should_Use_I_Prefix()
    {
        var offenders = AssemblyRefs.Core.GetTypes()
            .Where(t => t.IsInterface && t.IsPublic)
            .Where(t => !(t.Name.Length >= 2 && t.Name[0] == 'I' && char.IsUpper(t.Name[1])))
            .Select(t => t.FullName)
            .ToArray();

        Assert.That(offenders, Is.Empty,
            $"Core public interfaces should be ports and start with 'I'. Offenders: {string.Join(", ", offenders)}");
    }
}
