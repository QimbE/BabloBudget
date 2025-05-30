using BabloBudget.Api.Domain;

namespace BabloBudget.Api.Repository.Models;

public class AccountEntryDto
{
    public Guid Id { get; init; }

    public DateOnly DateUtc { get; init; }

    public required decimal Sum { get; init; }

    public required Guid? CategoryId { get; init; }

    public Guid AccountId { get; init; }

    public AccountEntry ToDomainModel(Category? category, Account account, IDateTimeProvider dateTimeProvider)
    {
        if (category?.Id != CategoryId)
            throw new ArgumentException("Category id must match given category", nameof(category));

        if (account.UserId != AccountId)
            throw new ArgumentException("Account id must match given account", nameof(category));

        var transaction = Transaction.Create(Money.Create(Sum), category);

        return AccountEntry.Create(Id, DateUtc, transaction, account, dateTimeProvider);
    }

    public static AccountEntryDto FromDomainModel(AccountEntry domainModel) =>
        new()
        {
            Id = domainModel.Id,
            DateUtc = domainModel.DateUtc,
            Sum = domainModel.Transaction.Sum.Amount,
            CategoryId = domainModel.Transaction.CategoryId,
            AccountId = domainModel.AccountId,
        };
}