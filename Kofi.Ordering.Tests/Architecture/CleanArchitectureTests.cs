using System.Reflection;
using System.Text.RegularExpressions;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Kofi.Ordering.Tests.Architecture;

public class CleanArchitectureTests
{
    private static readonly Assembly Implementation = Assembly.Load("Kofi.Ordering");

    private static readonly ArchUnitNET.Domain.Architecture Model = new ArchLoader()
        .LoadAssemblyIncludingDependencies(Implementation)
        .Build();

    [Theory]
    [InlineData("Domain", "Application")]
    [InlineData("Domain", "Infrastructure")]
    [InlineData("Domain", "API")]
    [InlineData("Application", "Infrastructure")]
    [InlineData("Application", "API")]
    [InlineData("Infrastructure", "API")]
    public void Layers_follow_the_dependency_direction(string source, string forbidden)
    {
        Types()
            .That()
            .Are(Layer(source))
            .Should()
            .NotDependOnAny(Layer(forbidden))
            .Check(Model);
    }

    [Theory]
    [InlineData(@"^(Microsoft\.EntityFrameworkCore|Npgsql\.EntityFrameworkCore)(\.|$)")]
    [InlineData(@"^Microsoft\.AspNetCore(\.|$)")]
    public void Domain_does_not_depend_on_persistence_or_web_frameworks(string frameworkNamespace)
    {
        Types()
            .That()
            .Are(Layer("Domain"))
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(frameworkNamespace))
            .Check(Model);
    }

    private static ArchUnitNET.Domain.IObjectProvider<ArchUnitNET.Domain.IType> Layer(string name) =>
        Types()
            .That()
            .ResideInAssembly(Implementation)
            .And()
            .ResideInNamespaceMatching("^" + Regex.Escape("Kofi.Ordering." + name) + @"(\.|$)")
            .As(name);
}
