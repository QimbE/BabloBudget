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

    public static Account Create(
        Money basisSum,
        Guid userId)
    {
        return new(basisSum, userId);
    }
}

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
}

public sealed record PeriodicalSchedule
{
    public DateOnly StartingDateUtc { get; init; }
    public DateOnly? LastCheckedUtc { get; init; }

    public Period Period { get; init; }

    private PeriodicalSchedule(
        Period period,
        DateOnly startingDateUtc,
        DateOnly? lastCheckedUtc)
    {
        Period = period;
        LastCheckedUtc = lastCheckedUtc;
        StartingDateUtc = startingDateUtc;
    }
    
    public PeriodicalSchedule MarkChecked(IDateTimeProvider dateTimeProvider) =>
        this with
        {
            LastCheckedUtc = dateTimeProvider.UtcNowDateOnly
        };

    public bool IsOnTime(IDateTimeProvider dateTimeProvider)
    {
        var currentDateUtc = dateTimeProvider.UtcNowDateOnly;

        return
            currentDateUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            - (LastCheckedUtc ?? StartingDateUtc).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            >= TimeSpan.FromDays(Period.Days);
    }
    
    public static PeriodicalSchedule New(DateOnly startingDateUtc, Period period, IDateTimeProvider dateTimeProvider)
    {
        var currentDateUtc = dateTimeProvider.UtcNowDateOnly;

        if (!StartsAtLeastTomorrow())
            throw new ArgumentOutOfRangeException(nameof(startingDateUtc), "Starting date is too early");

        return new PeriodicalSchedule(period, startingDateUtc, lastCheckedUtc: null);

        bool StartsAtLeastTomorrow()
        {
            return currentDateUtc.AddDays(1) <= startingDateUtc;
        }
    }

    public static PeriodicalSchedule Existing(
        DateOnly startingDateUtc,
        DateOnly? lastCheckedUtc,
        Period period,
        IDateTimeProvider dateTimeProvider)
    {
        if (lastCheckedUtc is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startingDateUtc, lastCheckedUtc.Value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(lastCheckedUtc.Value, dateTimeProvider.UtcNowDateOnly);
        }
        
        return new(period, startingDateUtc, lastCheckedUtc);
    }
}

public sealed record Period
{
    public const int Day = 1;
    public const int Week = 7;
    public const int Month = 30;

    private Period(int days)
    {
        Days = days;
    }

    public int Days { get; init; }

    public static Period CreateDaily() =>
        new(Day);
    
    public static Period CreateWeekly() =>
        new(Week);
    
    public static Period CreateMonthly() =>
        new(Month);

    public static Period FromDays(int days) =>
        days switch
        {
            Day => CreateDaily(),
            Week => CreateWeekly(),
            Month => CreateMonthly(),
            _ => throw new ArgumentOutOfRangeException(nameof(days), days, "Unsupported amount of days")
        };
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
        var currentDateUtc = dateTimeProvider.UtcNowDateOnly;
        
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