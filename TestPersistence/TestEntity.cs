using Core.Persistence.Entities;

namespace TestPersistence;

public class TestEntity : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}