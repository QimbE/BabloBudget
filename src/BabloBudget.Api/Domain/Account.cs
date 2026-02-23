namespace BabloBudget.Api.Domain;

public sealed record Account
{
    private Account(Money basisSum, Guid userId)
    {
        BasisSum = basisSum;
        UserId = userId;
    }

    public Money BasisSum { get; init; }

    public Guid UserId { get; init; }

    public static Account Create(
        Money basisSum,
        Guid userId)
    {
        return new(basisSum, userId);
    }
}