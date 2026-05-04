# FAZ-9: Kck.Pipeline.Mediator + Kck.Hosting.Aspire Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `Kck.Pipeline.Mediator` (source-generator based, AOT-native pipeline alternative) and `Kck.Hosting.Aspire` (service discovery + health checks) as additive v1.1 providers — zero breaking changes.

**Architecture:** Two new provider packages under `src/Providers/`. `Kck.Pipeline.Mediator` mirrors `Kck.Pipeline.MediatR` behavior-for-behavior using `martinothamar/Mediator` interfaces (`ValueTask`-returning). `Kck.Hosting.Aspire` wraps `Microsoft.Extensions.ServiceDiscovery` with a single `AddKckServiceDefaults()` entry point. `Kck.Pipeline.MediatR` is untouched; consumers choose which pipeline provider to use.

**Tech Stack:** .NET 8+10, martinothamar/Mediator.Abstractions 3.x, FluentValidation 12.x, Microsoft.Extensions.ServiceDiscovery 9.x, xUnit + FluentAssertions + NSubstitute

---

## File Map

| Action | Path |
|--------|------|
| Modify | `Directory.Packages.props` |
| Create | `src/Providers/Kck.Pipeline.Mediator/Kck.Pipeline.Mediator.csproj` |
| Create | `src/Providers/Kck.Pipeline.Mediator/KckMediatorPipelineBuilder.cs` |
| Create | `src/Providers/Kck.Pipeline.Mediator/Behaviors/ValidationBehavior.cs` |
| Create | `src/Providers/Kck.Pipeline.Mediator/Behaviors/LoggingBehavior.cs` |
| Create | `src/Providers/Kck.Pipeline.Mediator/Behaviors/CachingBehavior.cs` |
| Create | `src/Providers/Kck.Pipeline.Mediator/Behaviors/AuthorizationBehavior.cs` |
| Create | `src/Providers/Kck.Pipeline.Mediator/Behaviors/TransactionBehavior.cs` |
| Create | `src/Providers/Kck.Pipeline.Mediator/Behaviors/Log.cs` |
| Create | `src/Providers/Kck.Pipeline.Mediator/DependencyInjection/ServiceCollectionExtensions.cs` |
| Create | `tests/Kck.Pipeline.Mediator.Tests/Kck.Pipeline.Mediator.Tests.csproj` |
| Create | `tests/Kck.Pipeline.Mediator.Tests/ValidationBehaviorTests.cs` |
| Create | `tests/Kck.Pipeline.Mediator.Tests/CachingBehaviorTests.cs` |
| Create | `tests/Kck.Pipeline.Mediator.Tests/AuthorizationBehaviorTests.cs` |
| Create | `src/Providers/Kck.Hosting.Aspire/Kck.Hosting.Aspire.csproj` |
| Create | `src/Providers/Kck.Hosting.Aspire/DependencyInjection/HostApplicationBuilderExtensions.cs` |
| Create | `tests/Kck.Hosting.Aspire.Tests/Kck.Hosting.Aspire.Tests.csproj` |
| Create | `tests/Kck.Hosting.Aspire.Tests/AspireExtensionsTests.cs` |
| Modify | `OmerkckArchitecture.sln` (via `dotnet sln add`) |

---

## Task 1: Feature Branch + Package Versions

**Files:**
- Modify: `Directory.Packages.props`

- [ ] **Step 1: Create feature branch**

```bash
git checkout main && git pull origin main
git checkout -b feature/ls-faz9-pipeline-mediator-aspire
```

- [ ] **Step 2: Add Mediator.Abstractions to Directory.Packages.props**

Find the MediatR line:
```xml
<PackageVersion Include="MediatR" Version="14.1.0" />
```

Add after it:
```xml
<PackageVersion Include="Mediator.Abstractions" Version="3.1.0" />
```

- [ ] **Step 3: Add Microsoft.Extensions.ServiceDiscovery**

Find the Extensions block (near `Extensions.DependencyInjection.Abstractions`), add:
```xml
<PackageVersion Include="Microsoft.Extensions.ServiceDiscovery" Version="9.3.0" />
```

- [ ] **Step 4: Verify packages resolve**

```bash
dotnet restore --force-evaluate
```
Expected: no NU1004 or version errors.

- [ ] **Step 5: Commit**

```bash
git add Directory.Packages.props
git commit -m "chore(deps): add Mediator.Abstractions 3.1.0 and ServiceDiscovery 9.3.0"
```

---

## Task 2: Kck.Pipeline.Mediator — Project + Behaviors

**Files:**
- Create: `src/Providers/Kck.Pipeline.Mediator/Kck.Pipeline.Mediator.csproj`
- Create: `src/Providers/Kck.Pipeline.Mediator/Behaviors/ValidationBehavior.cs`
- Create: `src/Providers/Kck.Pipeline.Mediator/Behaviors/LoggingBehavior.cs`
- Create: `src/Providers/Kck.Pipeline.Mediator/Behaviors/CachingBehavior.cs`
- Create: `src/Providers/Kck.Pipeline.Mediator/Behaviors/AuthorizationBehavior.cs`
- Create: `src/Providers/Kck.Pipeline.Mediator/Behaviors/TransactionBehavior.cs`
- Create: `src/Providers/Kck.Pipeline.Mediator/Behaviors/Log.cs`

- [ ] **Step 1: Create csproj**

`src/Providers/Kck.Pipeline.Mediator/Kck.Pipeline.Mediator.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../Abstractions/Kck.Core.Abstractions/Kck.Core.Abstractions.csproj" />
    <ProjectReference Include="../../Abstractions/Kck.Authorization.Abstractions/Kck.Authorization.Abstractions.csproj" />
    <ProjectReference Include="../../Abstractions/Kck.Persistence.Abstractions/Kck.Persistence.Abstractions.csproj" />
    <ProjectReference Include="../../Abstractions/Kck.Exceptions.Abstractions/Kck.Exceptions.Abstractions.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Mediator.Abstractions" />
    <PackageReference Include="FluentValidation" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create ValidationBehavior**

`src/Providers/Kck.Pipeline.Mediator/Behaviors/ValidationBehavior.cs`:
```csharp
using FluentValidation;
using Kck.Exceptions;
using Kck.Exceptions.Models;
using Mediator;
using ValidationException = Kck.Exceptions.ValidationException;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Runs FluentValidation validators before the handler executes.
/// Throws <see cref="ValidationException"/> if any validator fails.
/// </summary>
public sealed class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (!validators.Any())
            return await next(message, cancellationToken);

        var context = new ValidationContext<TMessage>(message);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .Select(g => new ValidationExceptionModel
                {
                    Property = g.Key,
                    Errors = g.Select(e => e.ErrorMessage)
                })
                .ToList();

            throw new ValidationException(errors);
        }

        return await next(message, cancellationToken);
    }
}
```

- [ ] **Step 3: Create LoggingBehavior**

`src/Providers/Kck.Pipeline.Mediator/Behaviors/LoggingBehavior.cs`:
```csharp
using System.Diagnostics;
using Kck.Core.Abstractions.Pipeline;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Logs request execution timing for requests implementing <see cref="ILoggableRequest"/>.
/// Emits a warning when execution exceeds 500ms.
/// </summary>
public sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ILoggableRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var messageName = typeof(TMessage).Name;
        Log.HandlingRequest(logger, messageName);

        var sw = Stopwatch.StartNew();
        var response = await next(message, cancellationToken);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
            Log.LongRunningRequest(logger, messageName, sw.ElapsedMilliseconds);
        else
            Log.HandledRequest(logger, messageName, sw.ElapsedMilliseconds);

        return response;
    }
}
```

- [ ] **Step 4: Create CachingBehavior**

`src/Providers/Kck.Pipeline.Mediator/Behaviors/CachingBehavior.cs`:
```csharp
using System.Text.Json;
using Kck.Core.Abstractions.Pipeline;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Caches responses for requests implementing <see cref="ICachableRequest"/>.
/// Uses <see cref="IDistributedCache"/> for provider flexibility.
/// </summary>
public sealed class CachingBehavior<TMessage, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ICachableRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (message.BypassCache)
            return await next(message, cancellationToken);

        var cacheKey = message.CacheKey;

        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            Log.CacheHit(logger, cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        var response = await next(message, cancellationToken);

        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = message.SlidingExpiration ?? TimeSpan.FromMinutes(5)
        };

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), options, cancellationToken);
        Log.CacheSet(logger, cacheKey);
        return response;
    }
}
```

- [ ] **Step 5: Create AuthorizationBehavior**

`src/Providers/Kck.Pipeline.Mediator/Behaviors/AuthorizationBehavior.cs`:
```csharp
using Kck.Authorization.Abstractions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Exceptions;
using Mediator;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Enforces role-based authorization for requests implementing <see cref="ISecuredRequest"/>.
/// </summary>
public sealed class AuthorizationBehavior<TMessage, TResponse>(
    ICurrentUserProvider currentUser)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ISecuredRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        if (message.Roles.Length > 0 && !message.Roles.Any(r => currentUser.IsInRole(r)))
            throw new ForbiddenException($"Required roles: {string.Join(", ", message.Roles)}");

        return await next(message, cancellationToken);
    }
}
```

- [ ] **Step 6: Create TransactionBehavior**

`src/Providers/Kck.Pipeline.Mediator/Behaviors/TransactionBehavior.cs`:
```csharp
using Kck.Core.Abstractions.Pipeline;
using Kck.Persistence.Abstractions.UnitOfWork;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Wraps handler execution in a database transaction for requests implementing <see cref="ITransactionalRequest"/>.
/// </summary>
public sealed class TransactionBehavior<TMessage, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ITransactionalRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var messageName = typeof(TMessage).Name;
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        Log.TransactionStarted(logger, messageName);

        try
        {
            var response = await next(message, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            Log.TransactionCommitted(logger, messageName);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            Log.TransactionRolledBack(logger, messageName);
            throw;
        }
    }
}
```

- [ ] **Step 7: Create Log.cs**

`src/Providers/Kck.Pipeline.Mediator/Behaviors/Log.cs`:
```csharp
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {MessageName}")]
    public static partial void HandlingRequest(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long running request {MessageName} took {ElapsedMs}ms")]
    public static partial void LongRunningRequest(ILogger logger, string messageName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {MessageName} in {ElapsedMs}ms")]
    public static partial void HandledRequest(ILogger logger, string messageName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for {CacheKey}")]
    public static partial void CacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache set for {CacheKey}")]
    public static partial void CacheSet(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction started for {MessageName}")]
    public static partial void TransactionStarted(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction committed for {MessageName}")]
    public static partial void TransactionCommitted(ILogger logger, string messageName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Transaction rolled back for {MessageName}")]
    public static partial void TransactionRolledBack(ILogger logger, string messageName);
}
```

- [ ] **Step 8: Verify project builds**

```bash
cd src/Providers/Kck.Pipeline.Mediator
dotnet restore --force-evaluate
dotnet build -warnaserror
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

---

## Task 3: Kck.Pipeline.Mediator — Builder + DI Extension

**Files:**
- Create: `src/Providers/Kck.Pipeline.Mediator/KckMediatorPipelineBuilder.cs`
- Create: `src/Providers/Kck.Pipeline.Mediator/DependencyInjection/ServiceCollectionExtensions.cs`

- [ ] **Step 1: Create KckMediatorPipelineBuilder**

`src/Providers/Kck.Pipeline.Mediator/KckMediatorPipelineBuilder.cs`:
```csharp
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;

namespace Kck.Pipeline.Mediator;

/// <summary>
/// Fluent builder for registering KCK Mediator pipeline behaviors.
/// </summary>
public sealed class KckMediatorPipelineBuilder(IServiceCollection services)
{
    /// <summary>Gets the underlying service collection.</summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>Registers <see cref="ValidationBehavior{TMessage,TResponse}"/> for all messages.</summary>
    public KckMediatorPipelineBuilder UseValidationBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="LoggingBehavior{TMessage,TResponse}"/> for <c>ILoggableRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseLoggingBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="CachingBehavior{TMessage,TResponse}"/> for <c>ICachableRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseCachingBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="AuthorizationBehavior{TMessage,TResponse}"/> for <c>ISecuredRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseAuthorizationBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="TransactionBehavior{TMessage,TResponse}"/> for <c>ITransactionalRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseTransactionBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return this;
    }
}
```

- [ ] **Step 2: Create DI extension**

`src/Providers/Kck.Pipeline.Mediator/DependencyInjection/ServiceCollectionExtensions.cs`:
```csharp
using Kck.Pipeline.Mediator;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering KCK Mediator pipeline behaviors.
/// </summary>
public static class KckMediatorPipelineServiceCollectionExtensions
{
    /// <summary>
    /// Adds KCK Mediator pipeline behaviors via fluent builder.
    /// Call <c>services.AddMediator()</c> separately to register the source-generated dispatcher.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddMediator();
    /// services.AddKckMediatorPipeline(p => p
    ///     .UseValidationBehavior()
    ///     .UseLoggingBehavior()
    ///     .UseCachingBehavior());
    /// </code>
    /// </example>
    public static IServiceCollection AddKckMediatorPipeline(
        this IServiceCollection services,
        Action<KckMediatorPipelineBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        configure(new KckMediatorPipelineBuilder(services));
        return services;
    }
}
```

- [ ] **Step 3: Build full project**

```bash
dotnet build src/Providers/Kck.Pipeline.Mediator/Kck.Pipeline.Mediator.csproj -warnaserror
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

---

## Task 4: Kck.Pipeline.Mediator — Tests

**Files:**
- Create: `tests/Kck.Pipeline.Mediator.Tests/Kck.Pipeline.Mediator.Tests.csproj`
- Create: `tests/Kck.Pipeline.Mediator.Tests/ValidationBehaviorTests.cs`
- Create: `tests/Kck.Pipeline.Mediator.Tests/CachingBehaviorTests.cs`
- Create: `tests/Kck.Pipeline.Mediator.Tests/AuthorizationBehaviorTests.cs`

- [ ] **Step 1: Create test csproj**

`tests/Kck.Pipeline.Mediator.Tests/Kck.Pipeline.Mediator.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0</TargetFrameworks>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="NSubstitute" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Providers/Kck.Pipeline.Mediator/Kck.Pipeline.Mediator.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Write ValidationBehaviorTests (failing first)**

`tests/Kck.Pipeline.Mediator.Tests/ValidationBehaviorTests.cs`:
```csharp
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Kck.Exceptions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using NSubstitute;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class ValidationBehaviorTests
{
    private sealed record TestMessage : IMessage;

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        var sut = new ValidationBehavior<TestMessage, string>([]);
        var nextCalled = false;
        MessageHandlerDelegate<TestMessage, string> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult("result");
        };

        var result = await sut.Handle(new TestMessage(), CancellationToken.None, next);

        nextCalled.Should().BeTrue();
        result.Should().Be("result");
    }

    [Fact]
    public async Task Handle_WithPassingValidation_ShouldCallNext()
    {
        var validator = Substitute.For<IValidator<TestMessage>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var sut = new ValidationBehavior<TestMessage, string>([validator]);
        MessageHandlerDelegate<TestMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var result = await sut.Handle(new TestMessage(), CancellationToken.None, next);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShouldThrowValidationException()
    {
        var validator = Substitute.For<IValidator<TestMessage>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Required")]));
        var sut = new ValidationBehavior<TestMessage, string>([validator]);
        MessageHandlerDelegate<TestMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var act = async () => await sut.Handle(new TestMessage(), CancellationToken.None, next);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShouldNotCallNext()
    {
        var validator = Substitute.For<IValidator<TestMessage>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("X", "Error")]));
        var sut = new ValidationBehavior<TestMessage, string>([validator]);
        var nextCalled = false;
        MessageHandlerDelegate<TestMessage, string> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult("ok");
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => sut.Handle(new TestMessage(), CancellationToken.None, next).AsTask());

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MultipleValidators_ShouldAggregateErrors()
    {
        var v1 = Substitute.For<IValidator<TestMessage>>();
        v1.ValidateAsync(Arg.Any<ValidationContext<TestMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "TooShort")]));
        var v2 = Substitute.For<IValidator<TestMessage>>();
        v2.ValidateAsync(Arg.Any<ValidationContext<TestMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Email", "Invalid")]));

        var sut = new ValidationBehavior<TestMessage, string>([v1, v2]);
        MessageHandlerDelegate<TestMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => sut.Handle(new TestMessage(), CancellationToken.None, next).AsTask());

        ex.Errors.Should().HaveCount(2);
    }
}
```

- [ ] **Step 3: Run tests (should fail — no implementation compiled yet)**

```bash
dotnet test tests/Kck.Pipeline.Mediator.Tests --no-build 2>&1 | head -5
```
Expected: Build error (project not yet in solution). Proceed to Step 4.

- [ ] **Step 4: Write CachingBehaviorTests**

`tests/Kck.Pipeline.Mediator.Tests/CachingBehaviorTests.cs`:
```csharp
using System.Text.Json;
using FluentAssertions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class CachingBehaviorTests
{
    private sealed record CachableMessage(string Key, bool Bypass = false) : IRequest<string>, ICachableRequest
    {
        public string CacheKey => Key;
        public bool BypassCache => Bypass;
        public string? CacheGroupKey => null;
        public TimeSpan? SlidingExpiration => null;
    }

    private static IDistributedCache BuildCache() =>
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    [Fact]
    public async Task Handle_CacheMiss_ShouldCallNextAndCache()
    {
        var cache = BuildCache();
        var sut = new CachingBehavior<CachableMessage, string>(cache, NullLogger<CachingBehavior<CachableMessage, string>>.Instance);
        var nextCalled = false;
        MessageHandlerDelegate<CachableMessage, string> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult("computed");
        };

        var result = await sut.Handle(new CachableMessage("key1"), CancellationToken.None, next);

        nextCalled.Should().BeTrue();
        result.Should().Be("computed");
        var stored = await cache.GetStringAsync("key1");
        stored.Should().NotBeNull();
        JsonSerializer.Deserialize<string>(stored!).Should().Be("computed");
    }

    [Fact]
    public async Task Handle_CacheHit_ShouldReturnCachedValueWithoutCallingNext()
    {
        var cache = BuildCache();
        await cache.SetStringAsync("key2", JsonSerializer.Serialize("cached"), new DistributedCacheEntryOptions());
        var sut = new CachingBehavior<CachableMessage, string>(cache, NullLogger<CachingBehavior<CachableMessage, string>>.Instance);
        var nextCalled = false;
        MessageHandlerDelegate<CachableMessage, string> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult("fresh");
        };

        var result = await sut.Handle(new CachableMessage("key2"), CancellationToken.None, next);

        nextCalled.Should().BeFalse();
        result.Should().Be("cached");
    }

    [Fact]
    public async Task Handle_BypassCache_ShouldAlwaysCallNext()
    {
        var cache = BuildCache();
        await cache.SetStringAsync("key3", JsonSerializer.Serialize("cached"), new DistributedCacheEntryOptions());
        var sut = new CachingBehavior<CachableMessage, string>(cache, NullLogger<CachingBehavior<CachableMessage, string>>.Instance);
        MessageHandlerDelegate<CachableMessage, string> next = (_, _) => ValueTask.FromResult("fresh");

        var result = await sut.Handle(new CachableMessage("key3", Bypass: true), CancellationToken.None, next);

        result.Should().Be("fresh");
    }
}
```

- [ ] **Step 5: Write AuthorizationBehaviorTests**

`tests/Kck.Pipeline.Mediator.Tests/AuthorizationBehaviorTests.cs`:
```csharp
using FluentAssertions;
using Kck.Authorization.Abstractions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Exceptions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using NSubstitute;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class AuthorizationBehaviorTests
{
    private sealed record SecuredMessage(string[] RequiredRoles) : IRequest<string>, ISecuredRequest
    {
        public string[] Roles => RequiredRoles;
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorized()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(false);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);
        MessageHandlerDelegate<SecuredMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var act = async () => await sut.Handle(new SecuredMessage([]), CancellationToken.None, next);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_MissingRole_ShouldThrowForbidden()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        user.IsInRole("Admin").Returns(false);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);
        MessageHandlerDelegate<SecuredMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var act = async () => await sut.Handle(new SecuredMessage(["Admin"]), CancellationToken.None, next);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_HasRequiredRole_ShouldCallNext()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        user.IsInRole("Admin").Returns(true);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);
        MessageHandlerDelegate<SecuredMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var result = await sut.Handle(new SecuredMessage(["Admin"]), CancellationToken.None, next);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_NoRolesRequired_ShouldCallNext()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);
        MessageHandlerDelegate<SecuredMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var result = await sut.Handle(new SecuredMessage([]), CancellationToken.None, next);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_HasOneOfMultipleRoles_ShouldCallNext()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        user.IsInRole("Admin").Returns(false);
        user.IsInRole("Editor").Returns(true);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);
        MessageHandlerDelegate<SecuredMessage, string> next = (_, _) => ValueTask.FromResult("ok");

        var result = await sut.Handle(new SecuredMessage(["Admin", "Editor"]), CancellationToken.None, next);

        result.Should().Be("ok");
    }
}
```

---

## Task 5: Kck.Hosting.Aspire

**Files:**
- Create: `src/Providers/Kck.Hosting.Aspire/Kck.Hosting.Aspire.csproj`
- Create: `src/Providers/Kck.Hosting.Aspire/DependencyInjection/HostApplicationBuilderExtensions.cs`
- Create: `tests/Kck.Hosting.Aspire.Tests/Kck.Hosting.Aspire.Tests.csproj`
- Create: `tests/Kck.Hosting.Aspire.Tests/AspireExtensionsTests.cs`

- [ ] **Step 1: Create Aspire csproj**

`src/Providers/Kck.Hosting.Aspire/Kck.Hosting.Aspire.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create HostApplicationBuilderExtensions**

`src/Providers/Kck.Hosting.Aspire/DependencyInjection/HostApplicationBuilderExtensions.cs`:
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Aspire-oriented defaults for KCK services: service discovery, health checks,
/// and HTTP client default configuration.
/// </summary>
public static class KckAspireHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds KCK service defaults tuned for .NET Aspire:
    /// service discovery, health checks, and HTTP client service-discovery routing.
    /// </summary>
    /// <remarks>
    /// Call this before <c>AddKckObservability()</c> and <c>AddKckPipeline()</c>; those remain
    /// separate so projects not using Aspire are unaffected.
    /// </remarks>
    public static IHostApplicationBuilder AddKckServiceDefaults(
        this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
            http.AddServiceDiscovery());

        builder.Services.AddHealthChecks();

        return builder;
    }
}
```

- [ ] **Step 3: Create Aspire test csproj**

`tests/Kck.Hosting.Aspire.Tests/Kck.Hosting.Aspire.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0</TargetFrameworks>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Providers/Kck.Hosting.Aspire/Kck.Hosting.Aspire.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Write Aspire tests**

`tests/Kck.Hosting.Aspire.Tests/AspireExtensionsTests.cs`:
```csharp
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Kck.Hosting.Aspire.Tests;

public sealed class AspireExtensionsTests
{
    [Fact]
    public void AddKckServiceDefaults_ShouldRegisterHealthChecks()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddKckServiceDefaults();

        var app = builder.Build();
        app.Services.GetService(typeof(HealthCheckService)).Should().NotBeNull();
    }

    [Fact]
    public void AddKckServiceDefaults_CalledTwice_ShouldNotThrow()
    {
        var builder = Host.CreateApplicationBuilder();

        var act = () =>
        {
            builder.AddKckServiceDefaults();
            builder.AddKckServiceDefaults();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void AddKckServiceDefaults_NullBuilder_ShouldThrowArgumentNull()
    {
        IHostApplicationBuilder builder = null!;

        var act = () => builder.AddKckServiceDefaults();

        act.Should().Throw<ArgumentNullException>();
    }
}
```

---

## Task 6: SLN Registration + Lock Files + Final Verification

**Files:**
- Modify: `OmerkckArchitecture.sln`
- Regenerate: all `packages.lock.json`

- [ ] **Step 1: Add projects to solution**

```bash
dotnet sln add src/Providers/Kck.Pipeline.Mediator/Kck.Pipeline.Mediator.csproj
dotnet sln add tests/Kck.Pipeline.Mediator.Tests/Kck.Pipeline.Mediator.Tests.csproj
dotnet sln add src/Providers/Kck.Hosting.Aspire/Kck.Hosting.Aspire.csproj
dotnet sln add tests/Kck.Hosting.Aspire.Tests/Kck.Hosting.Aspire.Tests.csproj
```

Expected: `Project added to the solution.` for each.

- [ ] **Step 2: Regenerate lock files**

```bash
dotnet restore --force-evaluate
```

Expected: All projects restored, no NU1004 errors.

- [ ] **Step 3: Full solution build**

```bash
dotnet build -warnaserror
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Run all tests**

```bash
dotnet test --no-build -v q
```

Expected: All tests pass, no failures. New tests from Kck.Pipeline.Mediator.Tests and Kck.Hosting.Aspire.Tests included.

- [ ] **Step 5: Commit everything**

```bash
git add .
git commit -m "$(cat <<'EOF'
feat(pipeline): add Kck.Pipeline.Mediator provider and Kck.Hosting.Aspire

Kck.Pipeline.Mediator: martinothamar/Mediator-based pipeline alternative with
source-generator dispatch and ValueTask-returning behaviors (Validation, Logging,
Caching, Authorization, Transaction). Mirrors Kck.Pipeline.MediatR behavior-for-
behavior; consumers choose provider, no breaking changes.

Kck.Hosting.Aspire: AddKckServiceDefaults() extension on IHostApplicationBuilder
adding service discovery, HTTP client service-discovery routing, and health checks
for .NET Aspire environments.

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
EOF
)"
```

- [ ] **Step 6: Push and open PR**

```bash
git push -u origin feature/ls-faz9-pipeline-mediator-aspire
gh pr create \
  --title "LS-FAZ-9: Kck.Pipeline.Mediator + Kck.Hosting.Aspire (v1.1)" \
  --body "$(cat <<'EOF'
## Summary
- Add `Kck.Pipeline.Mediator` — martinothamar/Mediator based AOT-native pipeline provider
- Add `Kck.Hosting.Aspire` — `AddKckServiceDefaults()` for .NET Aspire environments
- Zero breaking changes; `Kck.Pipeline.MediatR` untouched

## Test Plan
- [ ] All existing tests still pass
- [ ] `Kck.Pipeline.Mediator.Tests`: ValidationBehavior (5), CachingBehavior (3), AuthorizationBehavior (5)
- [ ] `Kck.Hosting.Aspire.Tests`: AddKckServiceDefaults (3)
- [ ] CI: Build & Test + License Audit green

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```
