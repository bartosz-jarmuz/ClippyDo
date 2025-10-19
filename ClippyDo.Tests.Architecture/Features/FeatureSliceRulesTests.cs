using NetArchTest.Rules;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ClippyDo.Tests.Architecture.Features;

[Category("Architecture")]
public class FeatureSliceRulesTests
{
    private static IEnumerable<string> FeatureNames(Assembly asm, string rootNs)
        => asm.GetTypes()
              .Select(t => t.Namespace ?? "")
              .Where(ns => ns.StartsWith($"{rootNs}.Features."))
              .Select(ns => ns.Split('.')[3]) // ClippyDo . Core . Features . <Feature> . ...
              .Distinct();

    [Test]
    public void Core_Features_Must_Not_Cross_Depend()
    {
        var features = FeatureNames(AssemblyRefs.Core, AssemblyRefs.CoreNsRoot).ToArray();

        foreach (var feature in features)
        {
            var thisNs = $"{AssemblyRefs.CoreNsRoot}.Features.{feature}";
            var others = features.Where(f => f != feature).Select(f => $"{AssemblyRefs.CoreNsRoot}.Features.{f}").ToArray();

            var result = Types.InAssembly(AssemblyRefs.Core)
                .That().ResideInNamespace(thisNs)
                .Should().NotHaveDependencyOnAny(others)
                .GetResult();

            Assert.That(result.IsSuccessful, 
                $"Core feature '{feature}' depends on other Core features: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Test]
    public void Infrastructure_Features_Must_Not_Cross_Depend()
    {
        var features = FeatureNames(AssemblyRefs.Infrastructure, AssemblyRefs.InfraNsRoot).ToArray();

        foreach (var feature in features)
        {
            var thisNs = $"{AssemblyRefs.InfraNsRoot}.Features.{feature}";
            var others = features.Where(f => f != feature).Select(f => $"{AssemblyRefs.InfraNsRoot}.Features.{f}").ToArray();

            var result = Types.InAssembly(AssemblyRefs.Infrastructure)
                .That().ResideInNamespace(thisNs)
                .Should().NotHaveDependencyOnAny(others)
                .GetResult();

            Assert.That(result.IsSuccessful, Is.True,
                $"Infrastructure feature '{feature}' depends on other Infrastructure features: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }
}
