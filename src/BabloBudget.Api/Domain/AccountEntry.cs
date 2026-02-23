namespace BabloBudget.Api.Domain;

public sealed record AccountEntry
{
    private AccountEntry(Guid id, DateOnly dateUtc, Transaction transaction, Guid accountId)
    {
        Id = id;
        DateUtc = dateUtc;
        Transaction = transaction;
        AccountId = accountId;
    }

    public Guid Id { get; init; }
    public DateOnly DateUtc { get; init; }
    public Transaction Transaction { get; init; }
    public Guid AccountId { get; init; }

    public static AccountEntry Create(Guid id, DateOnly dateUtc, Transaction transaction, Account account, IDateTimeProvider dateTimeProvider)
    {
        var currentDateUtc = dateTimeProvider.UtcNowDateOnly;

        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateUtc, currentDateUtc);

        return new(id, dateUtc, transaction, account.UserId);
    }
}