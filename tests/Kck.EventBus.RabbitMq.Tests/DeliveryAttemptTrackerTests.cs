using AwesomeAssertions;
using Kck.EventBus.RabbitMq;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

public class DeliveryAttemptTrackerTests
{
    [Fact]
    public void Record_FirstCall_ReturnsOne()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());

        tracker.Record("m1").Should().Be(1);
    }

    [Fact]
    public void Record_SameKeyTwice_ReturnsTwo()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());

        tracker.Record("m1");

        tracker.Record("m1").Should().Be(2);
    }

    [Fact]
    public void Record_DifferentKeys_CountIndependently()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());

        tracker.Record("m1");
        tracker.Record("m1");

        tracker.Record("m2").Should().Be(1);
        tracker.Record("m1").Should().Be(3);
    }

    [Fact]
    public void Forget_ThenRecord_StartsAtOne()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());
        tracker.Record("m1");
        tracker.Record("m1");

        tracker.Forget("m1");

        tracker.Record("m1").Should().Be(1);
    }

    [Fact]
    public void Record_OverCapacity_PrunesEntriesOlderThanMaxAge()
    {
        var time = new TestTimeProvider();
        var tracker = new DeliveryAttemptTracker(time, maxTrackedMessages: 2, maxAge: TimeSpan.FromMinutes(1));
        tracker.Record("old");
        time.Advance(TimeSpan.FromMinutes(2));
        tracker.Record("fresh1");

        tracker.Record("fresh2"); // 3 entries > 2: "old" is older than maxAge and is pruned

        tracker.Count.Should().Be(2);
        tracker.Record("fresh1").Should().Be(2);
        tracker.Record("old").Should().Be(1);
    }

    [Fact]
    public void Record_OverCapacity_WithFreshEntries_EvictsOldestToCap()
    {
        var time = new TestTimeProvider();
        var tracker = new DeliveryAttemptTracker(time, maxTrackedMessages: 2, maxAge: TimeSpan.FromHours(1));
        tracker.Record("first");
        time.Advance(TimeSpan.FromSeconds(1));
        tracker.Record("second");
        time.Advance(TimeSpan.FromSeconds(1));

        tracker.Record("third"); // 3 entries > 2, none older than maxAge: the oldest ("first") is evicted

        tracker.Count.Should().Be(2);
        tracker.Record("second").Should().Be(2);
        tracker.Record("third").Should().Be(2);
    }

    [Fact]
    public void Record_OverCapacity_WithinPruneInterval_DoesNotScanAgain()
    {
        var time = new TestTimeProvider();
        var tracker = new DeliveryAttemptTracker(
            time,
            maxTrackedMessages: 2,
            maxAge: TimeSpan.FromHours(1),
            pruneInterval: TimeSpan.FromMinutes(1));
        tracker.Record("a");
        tracker.Record("b");
        tracker.Record("c"); // first prune: back to the cap
        tracker.Count.Should().Be(2);

        time.Advance(TimeSpan.FromSeconds(30));
        tracker.Record("d"); // over the cap, but the last prune was 30 s ago

        tracker.Count.Should().Be(3, "no prune runs within the prune interval");

        time.Advance(TimeSpan.FromSeconds(30));
        tracker.Record("e"); // the interval has elapsed: prune again

        tracker.Count.Should().Be(2);
    }
}

/// <summary>Manually advanced <see cref="TimeProvider"/> for deterministic timestamp tests.</summary>
internal sealed class TestTimeProvider : TimeProvider
{
    private long _timestamp = 1_000_000;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override long GetTimestamp() => Interlocked.Read(ref _timestamp);

    public void Advance(TimeSpan by) => Interlocked.Add(ref _timestamp, by.Ticks);
}
