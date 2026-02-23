namespace BabloBudget.Api.Domain;

public sealed record MoneyFlow
{
    private MoneyFlow(Guid id, Transaction transaction, PeriodicalSchedule schedule, Guid accountId)
    {
        Id = id;
        Transaction = transaction;
        Schedule = schedule;
        AccountId = accountId;
    }

    public Guid Id { get; init; }
    public Guid AccountId { get; init; }

    public Transaction Transaction { get; init; }
    public PeriodicalSchedule Schedule { get; init; }

    public static MoneyFlow Create(Guid id, Account account, Transaction transaction, PeriodicalSchedule schedule) =>
        new(id, transaction, schedule, account.UserId);

    public AccountEntry GetNextEntry(Guid idForEntry, Account account, IDateTimeProvider dateTimeProvider)
    {
        if (account.UserId != AccountId)
            throw new ArgumentException("Can not get entry for unknown account");

        return AccountEntry.Create(
            idForEntry,
            Schedule.NextCheckDateUtc,
            Transaction,
            account,
            dateTimeProvider);
    }

    public MoneyFlow? TryMarkProcessed(IDateTimeProvider dateTimeProvider)
    {
        var checkedSchedule = Schedule.TryMarkChecked(dateTimeProvider);
        
        if (checkedSchedule is null)
            return null;

        return this with
        {
            Schedule = checkedSchedule
        };
    }
}