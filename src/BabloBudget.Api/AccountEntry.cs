namespace BabloBudget.Api;

public static class AccountStrategies
{
    public static IQueryable<AccountEntry> GetAccountEntries(
        this IQueryable<AccountEntry> repository,
        Account account,
        DateOnly fromDate,
        DateOnly toDate) =>
        repository
            .Where(x => x.AccountId == account.UserId)
            .Where(x => x.DateUtc <= toDate)
            .Where(x => x.DateUtc >= fromDate);
}

public sealed record Account
{
    private Account(Money basisSum, Guid userId)
    {
        BasisSum = basisSum;
        UserId = userId;
    }

    public Money BasisSum { get; init; }
    
    public Guid UserId { get; init; }
    
    // Такто нафиг не надо тут держать ни Entries, ни Flows. Аккаунт - аггрегат, к остальному должен идти через репозитории по логике.
    // List<AccountEntry> Entries
    // ...
 

    public static Account Create(
        Money basisSum,
        Guid userId)
    {
        return new(basisSum, userId);
    }
}

public sealed record MoneyFlow
{
    private MoneyFlow(Guid id, Transaction transaction, Schedule schedule)
    {
        Id = id;
        Transaction = transaction;
        Schedule = schedule;
    }

    public Guid Id { get; init; }
    public Transaction Transaction { get; init; }
    
    // Id скедуле или вообще у скедуле id флоу
    public Schedule Schedule { get; init; }
}

public abstract record Schedule
{
    protected Schedule(Guid id, DateOnly startingDateUtc)
    {
        Id = id;
        StartingDateUtc = startingDateUtc;
    }

    public Guid Id { get; init; }
    public DateOnly StartingDateUtc { get; init; }
    public DateOnly? LastCheckedUtc { get; init; }
    
    public abstract bool IsOnTime(IDateTimeProvider dateTimeProvider);
    
    public Schedule MarkChecked(IDateTimeProvider dateTimeProvider) =>
        this with
        {
            LastCheckedUtc = DateOnly.FromDateTime(dateTimeProvider.UtcNow)
        };

    public static Schedule CreatePeriodical(Guid id, DateOnly startingDateUtc, Period period, IDateTimeProvider dateTimeProvider)
    {
        var currentDateUtc = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        if (currentDateUtc > startingDateUtc.AddDays(-1))
            throw new ArgumentOutOfRangeException(nameof(startingDateUtc), "Starting date is too early");

        return new PeriodicalSchedule(id, startingDateUtc, period);
    }
}

internal sealed record PeriodicalSchedule : Schedule
{
    public Period Period { get; init; }
    
    internal PeriodicalSchedule(Guid id, DateOnly startingDateUtc, Period period)
        : base(id, startingDateUtc)
    {
        Period = period;
    }

    public override bool IsOnTime(IDateTimeProvider dateTimeProvider)
    {
        var currentDateUtc = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        return
            currentDateUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            - (LastCheckedUtc ?? StartingDateUtc).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            >= Period.Span;
    }
}

public sealed record Period
{
    private Period(TimeSpan span)
    {
        Span = span;
    }

    public TimeSpan Span { get; init; }

    public static Period Create(TimeSpan span)
    {
        if (span <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(span), "Span must be positive");

        return new(span);
    }
}

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
        var currentDateUtc = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateUtc, currentDateUtc);
        
        return new(id, dateUtc, transaction, account.UserId);
    }
}

public sealed record Transaction
{
    private Transaction(Money sum, Guid categoryId)
    {
        Sum = sum;
        CategoryId = categoryId;
    }

    public Money Sum { get; init; }

    public Guid CategoryId { get; init; }

    public static Transaction Create(Money sum, Category category)
    {
        ArgumentOutOfRangeException.ThrowIfZero(sum.Amount);
        
        if (sum.IsNegative && category.Type is not CategoryType.Expense)
            throw new ArgumentException("Can not use non-expense category for expense transaction");
        
        if (sum.IsPositive && category.Type is not CategoryType.Income)
            throw new ArgumentException("Can not use non-income category for income transaction");

        return new(sum, category.Id);
    }
    
    public Money Apply(Money sourceSum) =>
        sourceSum + Sum;
}


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

public sealed record Category
{
    private Category(Guid id, string name, CategoryType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    public Guid Id { get; init; }
    public string Name { get; init; }
    
    public CategoryType Type { get; init; }

    public static Category Create(Guid id, string name, CategoryType type)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(name.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, 50);

        return new(id, name, type);
    }
}

public enum CategoryType
{
    Expense = 0,
    Income = 1
}