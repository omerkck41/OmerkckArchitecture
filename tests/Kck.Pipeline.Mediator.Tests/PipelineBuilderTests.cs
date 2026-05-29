using FluentAssertions;
using Kck.Pipeline.Mediator;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class PipelineBuilderTests
{
    [Fact]
    public void UseBehaviors_RegisterAllOpenGenericBehaviorsAsTransient()
    {
        var services = new ServiceCollection();
        var builder = new KckMediatorPipelineBuilder(services);

        // Fluent chaining also proves each Use* method returns the builder (a null return breaks the chain).
        builder
            .UseValidationBehavior()
            .UseLoggingBehavior()
            .UseCachingBehavior()
            .UseAuthorizationBehavior()
            .UseTransactionBehavior();

        void AssertRegistered(Type impl) =>
            services.Should().Contain(d =>
                d.ServiceType == typeof(IPipelineBehavior<,>) &&
                d.ImplementationType == impl &&
                d.Lifetime == ServiceLifetime.Transient);

        AssertRegistered(typeof(ValidationBehavior<,>));
        AssertRegistered(typeof(LoggingBehavior<,>));
        AssertRegistered(typeof(CachingBehavior<,>));
        AssertRegistered(typeof(AuthorizationBehavior<,>));
        AssertRegistered(typeof(TransactionBehavior<,>));
    }

    [Fact]
    public void UseValidationBehavior_ReturnsSameBuilderInstanceForChaining()
    {
        var builder = new KckMediatorPipelineBuilder(new ServiceCollection());

        builder.UseValidationBehavior().Should().BeSameAs(builder);
    }

    [Fact]
    public void Services_ExposesUnderlyingServiceCollection()
    {
        var services = new ServiceCollection();
        var builder = new KckMediatorPipelineBuilder(services);

        builder.Services.Should().BeSameAs(services);
    }

    [Fact]
    public void AddKckMediatorPipeline_InvokesConfigureAndReturnsServices()
    {
        var services = new ServiceCollection();
        var configureCalled = false;

        var result = services.AddKckMediatorPipeline(p =>
        {
            configureCalled = true;
            p.UseValidationBehavior();
        });

        configureCalled.Should().BeTrue();
        result.Should().BeSameAs(services);
        services.Should().Contain(d => d.ServiceType == typeof(IPipelineBehavior<,>));
    }

    [Fact]
    public void AddKckMediatorPipeline_NullServices_Throws()
    {
        IServiceCollection services = null!;

        var act = () => services.AddKckMediatorPipeline(_ => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddKckMediatorPipeline_NullConfigure_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckMediatorPipeline(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
