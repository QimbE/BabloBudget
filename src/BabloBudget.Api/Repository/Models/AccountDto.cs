using BabloBudget.Api.Domain;

namespace BabloBudget.Api.Repository.Models;

public sealed class AccountDto
{
    public required Guid Id { get; init; }
    
    public required decimal BasisSum { get; init; }

    public static AccountDto FromDomainModel(Account account)
    {
        return new AccountDto()
        {
            BasisSum = account.BasisSum.Amount,
            Id = account.UserId,
        };
    }

    public Account ToDomainModel()
    {
        var money = Money.Create(BasisSum);
        return Account.Create(money, Id);
    }
}