using BabloBudget.Api.Domain;

namespace BabloBudget.Api.Repository.Models;

public sealed class MoneyFlowDto
{
    public required Guid Id { get; init; }
    
    public required Guid AccountId { get; init; }
    
    
    public required Guid? CategoryId { get; init; }
    
    public required decimal Sum { get; init; }
    
    
    public required DateOnly StartingDateUtc { get; init; }

    public required DateOnly? LastCheckedUtc { get; init; }
    
    public required int PeriodDays { get; init; }

    public MoneyFlow ToDomainModel(Account account, Category? category, IDateTimeProvider dateTimeProvider)
    {
        if (category?.Id != CategoryId)
            throw new ArgumentException("Category id must match given category", nameof(category));
        
        if (account.UserId != AccountId)
            throw new ArgumentException("Account id must match given account", nameof(category));
        
        
        var schedule = PeriodicalSchedule.Existing(
            StartingDateUtc,
            LastCheckedUtc,
            Period.FromDays(PeriodDays),
            dateTimeProvider);
        
        var transaction = Transaction.Create(Money.Create(Sum), category);
        
        return MoneyFlow.Create(Id, account, transaction, schedule);
    }

    public MoneyFlowDto FromDomainModel(MoneyFlow domainModel) =>
        new()
        {
            Id = domainModel.Id,
            AccountId = domainModel.AccountId,
            CategoryId = domainModel.Transaction.CategoryId,
            Sum = domainModel.Transaction.Sum.Amount,
            StartingDateUtc = domainModel.Schedule.StartingDateUtc,
            LastCheckedUtc = domainModel.Schedule.LastCheckedUtc,
            PeriodDays = domainModel.Schedule.Period.Days
        };
}