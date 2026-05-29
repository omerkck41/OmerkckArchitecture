using FluentAssertions;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Pipeline.Abstractions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using NSubstitute;
using Xunit;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class TransactionBehaviorTests
{
    private sealed record TxMessage : IRequest<string>, ITransactionalRequest;

    [Fact]
    public async Task Handle_Success_Begins_Saves_Commits_AndLogs()
    {
        var uow = Substitute.For<IUnitOfWork>();
        var logger = new CapturingLogger<TransactionBehavior<TxMessage, string>>();
        var sut = new TransactionBehavior<TxMessage, string>(uow, logger);

        var result = await sut.Handle(new TxMessage(), (_, _) => ValueTask.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
        Received.InOrder(() =>
        {
            uow.BeginTransactionAsync(Arg.Any<CancellationToken>());
            uow.SaveChangesAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>());
            uow.CommitAsync(Arg.Any<CancellationToken>());
        });
        await uow.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
        logger.Entries.Should().Contain(e => e.Contains("started"));
        logger.Entries.Should().Contain(e => e.Contains("committed"));
    }

    [Fact]
    public async Task Handle_HandlerThrows_RollsBack_Rethrows_AndLogs()
    {
        var uow = Substitute.For<IUnitOfWork>();
        var logger = new CapturingLogger<TransactionBehavior<TxMessage, string>>();
        var sut = new TransactionBehavior<TxMessage, string>(uow, logger);

        var act = () => sut.Handle(
            new TxMessage(),
            (_, _) => throw new InvalidOperationException("boom"),
            CancellationToken.None).AsTask();

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("boom");
        await uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        logger.Entries.Should().Contain(e => e.Contains("rolled back"));
    }
}
