namespace BabloBudget.Api.Domain;

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
