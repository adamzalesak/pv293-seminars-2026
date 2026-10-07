using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Kofi.Subscriptions.Tests.Architecture;

public class ModuleBoundaryTests
{
    private static readonly Assembly Implementation = Assembly.Load("Kofi.Subscriptions");
    private static readonly Assembly Contracts = Assembly.Load("Kofi.Subscriptions.Contracts");

    private static readonly Assembly[] ModuleImplementations =
    [
        Assembly.Load("Kofi.Catalog"),
        Assembly.Load("Kofi.Warehouse"),
        Assembly.Load("Kofi.Ordering"),
        Assembly.Load("Kofi.Subscriptions"),
        Assembly.Load("Kofi.Marketing")
    ];

    private static readonly ArchUnitNET.Domain.Architecture Model = new ArchLoader()
        .LoadAssemblies(ModuleImplementations.Concat(new[] { Contracts, Assembly.Load("Kofi.Common") }).ToArray())
        .Build();

    [Theory]
    [InlineData("Kofi.Catalog")]
    [InlineData("Kofi.Warehouse")]
    [InlineData("Kofi.Ordering")]
    [InlineData("Kofi.Marketing")]
    public void Module_uses_foreign_contracts_instead_of_implementation(string foreignAssembly)
    {
        Types()
            .That()
            .ResideInAssembly(Implementation)
            .Should()
            .NotDependOnAny(Types().That().ResideInAssembly(Assembly.Load(foreignAssembly)))
            .Check(Model);
    }

    [Fact]
    public void Contracts_do_not_expose_module_implementation_or_shared_kernel()
    {
        Types()
            .That()
            .ResideInAssembly(Contracts)
            .Should()
            .NotDependOnAny(Types()
                .That()
                .ResideInAssembly(
                    ModuleImplementations[0], ModuleImplementations.Skip(1).Append(Assembly.Load("Kofi.Common")).ToArray()))
            .WithoutRequiringPositiveResults()
            .Check(Model);
    }
}
