namespace BabloBudget.Api.Domain;

public sealed record Money
{
    private Money(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; init; }

    public bool IsNegative => Amount < 0;

    public bool IsPositive => Amount > 0;

    public static Money operator +(Money a, Money b)
    {
        return Create(a.Amount + b.Amount);
    }

    public static Money Create(decimal amount)
    {
        return new(amount);
    }
}
