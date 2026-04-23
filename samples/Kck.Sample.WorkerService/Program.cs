using Kck.BackgroundJobs.Abstractions;
using Kck.Sample.WorkerService.Jobs;

var builder = Host.CreateApplicationBuilder(args);

// Register jobs in DI
builder.Services.AddTransient<SampleRecurringJob>();
builder.Services.AddTransient<SampleCleanupJob>();

// Add Hangfire with in-memory storage
builder.Services.AddKckBackgroundJobsHangfire(opts =>
{
    opts.StorageType = "InMemory";
    opts.WorkerCount = 2;
});

var host = builder.Build();

// Register recurring jobs
var scheduler = host.Services.GetRequiredService<IJobScheduler>();
await scheduler.AddOrUpdateRecurringAsync<SampleRecurringJob>(
    "sample-recurring", CronExpressions.Every5Minutes);

host.Run();
