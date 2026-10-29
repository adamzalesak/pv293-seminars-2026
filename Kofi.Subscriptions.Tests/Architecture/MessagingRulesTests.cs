using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Kofi.Common.Abstractions;
using MediatR;
using Wolverine;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Kofi.Subscriptions.Tests.Architecture;

// Commands and queries run on MediatR, events on Wolverine, each in its own
// transaction. Reaching for the other bus from a handler would open a second
// transaction, or send a message outside the one the handler runs in.
public class MessagingRulesTests
{
    private static readonly Assembly Implementation = Assembly.Load("Kofi.Subscriptions");

    private static readonly ArchUnitNET.Domain.Architecture Model = new ArchLoader()
        .LoadAssemblyIncludingDependencies(Implementation)
        .Build();

    [Fact]
    public void Wolverine_handlers_do_not_use_MediatR()
    {
        Classes()
            .That()
            .ResideInAssembly(Implementation)
            .And()
            .FollowCustomPredicate(c => IsWolverineHandler(Implementation.GetType(c.FullName)), "are Wolverine handlers")
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^MediatR(\.|$)"))
            .WithoutRequiringPositiveResults()
            .Check(Model);
    }

    // As the host finds them: our event handlers, sagas, and any other class
    // whose name ends in Handler, except MediatR's.
    private static bool IsWolverineHandler(System.Type? type) =>
        type != null && !type.IsAbstract &&
        (Implements(type, typeof(IDomainEventHandler<>), typeof(IIntegrationEventHandler<>)) || typeof(Saga).IsAssignableFrom(type) ||
         (type.Name.EndsWith("Handler") && !Implements(type, typeof(IRequestHandler<>), typeof(IRequestHandler<,>))));

    private static bool Implements(System.Type type, params System.Type[] generics) =>
        type.GetInterfaces().Any(i => i.IsGenericType && generics.Contains(i.GetGenericTypeDefinition()));

    [Fact]
    public void Command_and_query_handlers_do_not_use_the_Wolverine_bus()
    {
        Classes()
            .That()
            .ResideInAssembly(Implementation)
            .And()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Or()
            .ResideInAssembly(Implementation)
            .And()
            .ImplementInterface(typeof(IRequestHandler<>))
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^Wolverine(\.|$)"))
            .WithoutRequiringPositiveResults()
            .Check(Model);
    }
}
