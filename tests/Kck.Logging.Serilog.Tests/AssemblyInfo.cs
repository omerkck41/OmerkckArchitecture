using Xunit;

// AddKckSerilog builds loggers that touch process-wide Serilog state (static Log.Logger); running the
// registration test classes concurrently makes event capture non-deterministic.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
