using Kck.Exceptions;
using Kck.Exceptions.AspNetCore.Extensions;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Sample.MinimalApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Services.AddKckSerilog(serilog => serilog
    .WriteToConsole());

// Persistence — EF Core InMemory
builder.Services.AddKckPersistence(persistence => persistence
    .UseEntityFramework<SampleDbContext>(db => db.UseInMemoryDatabase("SampleDb"))
    .AddAuditInterceptor());

// Exception handling — RFC 7807
builder.Services.AddKckExceptionHandling();

var app = builder.Build();

app.UseKckExceptionHandling();

// --- Endpoints ---

app.MapGet("/todos", async (SampleDbContext db, CancellationToken ct) =>
{
    var items = await db.TodoItems.ToListAsync(ct);
    return Results.Ok(items);
});

app.MapGet("/todos/{id:guid}", async (Guid id, SampleDbContext db, CancellationToken ct) =>
{
    var item = await db.TodoItems.FindAsync([id], ct)
        ?? throw new NotFoundException($"TodoItem with id '{id}' was not found.");
    return Results.Ok(item);
});

app.MapPost("/todos", async (CreateTodoRequest request, SampleDbContext db, CancellationToken ct) =>
{
    var item = new TodoItem
    {
        Id = Guid.CreateVersion7(),
        Title = request.Title
    };

    db.TodoItems.Add(item);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/todos/{item.Id}", item);
});

app.MapPut("/todos/{id:guid}/done", async (Guid id, SampleDbContext db, CancellationToken ct) =>
{
    var item = await db.TodoItems.FindAsync([id], ct)
        ?? throw new NotFoundException($"TodoItem with id '{id}' was not found.");

    item.IsDone = true;
    await db.SaveChangesAsync(ct);
    return Results.Ok(item);
});

app.MapDelete("/todos/{id:guid}", async (Guid id, SampleDbContext db, CancellationToken ct) =>
{
    var item = await db.TodoItems.FindAsync([id], ct)
        ?? throw new NotFoundException($"TodoItem with id '{id}' was not found.");

    db.TodoItems.Remove(item);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

app.Run();

