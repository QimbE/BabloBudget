namespace BabloBudget.Api;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    
    DateOnly UtcNowDateOnly { get; }
}

internal sealed class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly UtcNowDateOnly => DateOnly.FromDateTime(UtcNow);
}