using BabloBudget.Api;

namespace Tests;

internal sealed class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => _dateTimeProvider();
    public DateOnly UtcNowDateOnly => DateOnly.FromDateTime(UtcNow);
    
    private readonly Func<DateTime> _dateTimeProvider;

    private TestDateTimeProvider(Func<DateTime> dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }
    
    internal static IDateTimeProvider Create(DateTime dateTimeToProvide) =>
        new TestDateTimeProvider(() => dateTimeToProvide);
}