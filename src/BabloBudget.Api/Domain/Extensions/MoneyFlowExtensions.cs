namespace BabloBudget.Api.Domain.Extensions;

public static class MoneyFlowExtensions
{
    public static IEnumerable<(MoneyFlow MoneyFlow, AccountEntry Entry)> MakeNextEntries(
        this IEnumerable<(MoneyFlow MoneyFlow, Account Account)> moneyFlowsWithAccounts,
        IDateTimeProvider dateTimeProvider) =>
        moneyFlowsWithAccounts
            .Where(mf => mf.MoneyFlow.Schedule.IsOnTime(dateTimeProvider))
            .Select(mf => (mf.MoneyFlow,
                Entry: mf.MoneyFlow.GetNextEntry(Guid.NewGuid(), mf.Account, dateTimeProvider)))
            .Select(mf => (MoneyFlow: mf.MoneyFlow.TryMarkProcessed(dateTimeProvider), mf.Entry))
            .Where(mf => mf.MoneyFlow is not null)
            .Select(mf => (MoneyFlow: mf.MoneyFlow!, mf.Entry));
}