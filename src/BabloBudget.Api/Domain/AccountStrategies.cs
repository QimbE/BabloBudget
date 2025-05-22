namespace BabloBudget.Api.Domain;

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