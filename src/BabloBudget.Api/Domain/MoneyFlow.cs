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
}

public sealed record PeriodicalSchedule
{
    public DateOnly StartingDateUtc { get; init; }
    public DateOnly? LastCheckedUtc { get; init; }

    public Period Period { get; init; }

    public DateOnly NextCheckDateUtc =>
        LastCheckedUtc is null
            ? StartingDateUtc
            : LastCheckedUtc!.Value.AddDays(Period.Days);


    private PeriodicalSchedule(
        Period period,
        DateOnly startingDateUtc,
        DateOnly? lastCheckedUtc)
    {
        Period = period;
        LastCheckedUtc = lastCheckedUtc;
        StartingDateUtc = startingDateUtc;
    }

    public PeriodicalSchedule? TryMarkChecked(IDateTimeProvider dateTimeProvider)
    {
        if (!IsOnTime(dateTimeProvider))
            return null;
        
        return this with
        {
            LastCheckedUtc = NextCheckDateUtc
        };
    }

    public bool IsOnTime(IDateTimeProvider dateTimeProvider)
    {
        var currentDateUtc = dateTimeProvider.UtcNowDateOnly;

        return currentDateUtc >= NextCheckDateUtc;
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
        if (lastCheckedUtc is null)
            return new(period, startingDateUtc, lastCheckedUtc);

        ArgumentOutOfRangeException.ThrowIfGreaterThan(startingDateUtc, lastCheckedUtc.Value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(lastCheckedUtc.Value, dateTimeProvider.UtcNowDateOnly);

        if (!IsOnSchedule(startingDateUtc, lastCheckedUtc.Value, period))
            throw new ArgumentOutOfRangeException(nameof(lastCheckedUtc), "Impossible last checked date");

        return new(period, startingDateUtc, lastCheckedUtc);
    }

    private static bool IsOnSchedule(DateOnly startingDateUtc, DateOnly lastCheckedUtc, Period period)
    {
        if (lastCheckedUtc < startingDateUtc)
            return false;

        return (lastCheckedUtc.DayNumber - startingDateUtc.DayNumber) % period.Days == 0;
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