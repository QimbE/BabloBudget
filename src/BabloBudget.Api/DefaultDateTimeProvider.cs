namespace BabloBudget.Api;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

internal sealed class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}