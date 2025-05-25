namespace BabloBudget.Api.Domain;

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
