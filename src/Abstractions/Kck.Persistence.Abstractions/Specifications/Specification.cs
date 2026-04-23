using System.Linq.Expressions;

namespace Kck.Persistence.Abstractions.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];
    private readonly List<string> _includeStrings = [];

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();
    public IReadOnlyList<string> IncludeStrings => _includeStrings.AsReadOnly();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool WithDeleted { get; private set; }
    public bool EnableTracking { get; private set; }

    protected void Where(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    protected void AddInclude(Expression<Func<T, object>> includeExpression) => _includes.Add(includeExpression);

    protected void AddInclude(string includeString) => _includeStrings.Add(includeString);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescending) => OrderByDescending = orderByDescending;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    protected void IncludeDeleted() => WithDeleted = true;

    protected void AsTracking() => EnableTracking = true;
}
