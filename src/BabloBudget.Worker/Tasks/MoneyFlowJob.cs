using BabloBudget.Api;
using BabloBudget.Api.Common;
using BabloBudget.Api.Domain;
using BabloBudget.Api.Domain.Extensions;
using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Extensions;
using BabloBudget.Api.Repository.Models;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.EntityFrameworkCore;

namespace BabloBudget.Worker.Tasks;

public sealed class MoneyFlowJob(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IDateTimeProvider dateTimeProvider) : IJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken) =>
        await dbContextFactory.ExecuteAndCommitAsync(
            dbContext => ProcessNextMoneyFlowsAsync(dbContext, cancellationToken),
            cancellationToken: cancellationToken);

    private async Task ProcessNextMoneyFlowsAsync(ApplicationDbContext dbContext, CancellationToken token)
    {
        var onTimeFlowDetailDtos = await GetOnTimeFlowsAsync(
            dbContext,
            dateTimeProvider,
            token);

        var onTimeMoneyFlows = onTimeFlowDetailDtos
            .Select(mfd =>
                mfd.ToDomainModels(dateTimeProvider));
        
        var processedFlows = onTimeMoneyFlows.MakeNextEntries(dateTimeProvider);
        
        var (moneyFlowsToUpdate, accountEntriesToAdd) = processedFlows
            .Select(mf =>
                (MoneyFlowDto.FromDomainModel(mf.MoneyFlow), AccountEntryDto.FromDomainModel(mf.Entry)))
            .UnZip();
        
        dbContext.MoneyFlows.UpdateRange(moneyFlowsToUpdate);
        dbContext.AccountEntries.AddRange(accountEntriesToAdd);
        
        await dbContext.SaveChangesAsync(token);
    }

    private static async Task<IReadOnlyCollection<MoneyFlowDetailDto>> GetOnTimeFlowsAsync(
        ApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken)
    {
        var moneyFlows = dbContext.MoneyFlows.GetOnTimeFlows(dateTimeProvider.UtcNowDateOnly);

        // the fuck?
        var fromDb = await moneyFlows
            .Join(dbContext.Accounts,
                mf => mf.AccountId,
                acc => acc.Id,
                (mf, acc) => new { MoneyFlow = mf, Account = acc })
            .GroupJoin(dbContext.Categories,
                mfAcc => mfAcc.MoneyFlow.CategoryId,
                cat => cat.Id,
                (mfAcc, cats) => new { mfAcc.MoneyFlow, mfAcc.Account, Category = cats.FirstOrDefault() })
            .ToListAsync(cancellationToken);

        return fromDb
            .Select(m => new MoneyFlowDetailDto(m.MoneyFlow, m.Account, m.Category))
            .ToList();
    }
            
    private record MoneyFlowDetailDto(
        MoneyFlowDto MoneyFlow,
        AccountDto Account,
        CategoryDto? Category)
    {
        public (MoneyFlow moneyFlow, Account account) ToDomainModels(IDateTimeProvider dateTimeProvider)
        {
            var account = Account.ToDomainModel();
            var category = Category?.ToDomainModel();
            var moneyFlow = MoneyFlow.ToDomainModel(account, category, dateTimeProvider);
        
            return (moneyFlow, account);
        }
    }
}

