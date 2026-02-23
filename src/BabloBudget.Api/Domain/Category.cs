namespace BabloBudget.Api.Domain;

public sealed record Category
{
    private Category(Guid id, Guid userId, string name, CategoryType type)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Type = type;
    }

    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string Name { get; init; }

    public CategoryType Type { get; init; }

    public static Category Create(Guid id, Guid userId, string name, CategoryType type)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(name.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, 50);

        return new(id, userId, name, type);
    }
}

public enum CategoryType
{
    Expense = 0,
    Income = 1
}
