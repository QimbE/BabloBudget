namespace BabloBudget.Api.Domain;

public sealed record Transaction
{
    private Transaction(Money sum, Guid? categoryId)
    {
        Sum = sum;
        CategoryId = categoryId;
    }

    public Money Sum { get; init; }

    public Guid? CategoryId { get; init; }

    public static Transaction Create(Money sum, Category? category)
    {
        ArgumentOutOfRangeException.ThrowIfZero(sum.Amount);

        if (category is null)
            return new(sum, null);

        if (sum.IsNegative && category.Type is not CategoryType.Expense)
            throw new ArgumentException("Can not use non-expense category for expense transaction");

        if (sum.IsPositive && category.Type is not CategoryType.Income)
            throw new ArgumentException("Can not use non-income category for income transaction");

        return new(sum, category.Id);
    }

    public Money Apply(Money sourceSum) =>
        sourceSum + Sum;
}