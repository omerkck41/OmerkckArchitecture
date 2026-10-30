using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Kck.Sample.Orders.Features.Customers;

public sealed record CreateCustomerRequest(string Name);

public sealed record CustomerResponse(Guid Id, string Name, bool IsDeleted, DateTime CreatedDate, DateTime? DeletedDate)
{
    public static CustomerResponse From(Customer customer) =>
        new(customer.Id, customer.Name, customer.IsDeleted, customer.CreatedDate, customer.DeletedDate);
}

/// <summary>Create, read and delete customers. Deleting is a soft delete, refused while the customer has orders.</summary>
public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var customers = app.MapGroup("/customers").WithTags("Customers").RequireAuthorization();
        customers.MapPost("/", CreateAsync).WithName("CreateCustomer");
        customers.MapGet("/{id:guid}", GetAsync).WithName("GetCustomer");
        customers.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteCustomer");
        return app;
    }

    public static async Task<Results<Created<CustomerResponse>, ValidationProblem>> CreateAsync(
        CreateCustomerRequest request,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > Customer.NameMaxLength)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Name)] = [$"A name of 1 to {Customer.NameMaxLength} characters is required."],
            });
        }

        var customer = new Customer { Id = Guid.CreateVersion7(), Name = request.Name.Trim() };
        await unitOfWork.Repository<Customer, Guid>().AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        return TypedResults.Created($"/customers/{customer.Id}", CustomerResponse.From(customer));
    }

    public static async Task<Results<Ok<CustomerResponse>, NotFound>> GetAsync(
        Guid id,
        bool? includeDeleted,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var options = includeDeleted == true ? QueryOptions.WithDeleted : QueryOptions.None;
        var customer = await unitOfWork.Repository<Customer, Guid>()
            .GetAsync(c => c.Id == id, options, cancellationToken: cancellationToken);

        return customer is null ? TypedResults.NotFound() : TypedResults.Ok(CustomerResponse.From(customer));
    }

    /// <summary>
    /// A soft delete of a customer without orders. Orders reference customers with <c>DeleteBehavior.Restrict</c>: a
    /// customer with orders cannot be deleted (409), so no order ever points at a deleted customer.
    /// </summary>
    public static async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> DeleteAsync(
        Guid id,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var customers = unitOfWork.Repository<Customer, Guid>();
        var customer = await customers.GetAsync(c => c.Id == id, cancellationToken: cancellationToken);
        if (customer is null)
            return TypedResults.NotFound();

        // Restrict is checked here rather than left to EF: a soft delete applies the delete rules only to tracked
        // (loaded) dependants, as EF's cascade does, and EF reports a restricted delete with a generic exception.
        // Cancelled orders count too: they still reference the customer.
        if (await unitOfWork.Repository<Order, Guid>().AnyAsync(QueryOptions.WithDeleted, o => o.CustomerId == id, cancellationToken))
        {
            return TypedResults.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Customer has orders",
                Detail = "A customer with orders cannot be deleted.",
            });
        }

        await customers.DeleteAsync(customer, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        return TypedResults.NoContent();
    }
}
