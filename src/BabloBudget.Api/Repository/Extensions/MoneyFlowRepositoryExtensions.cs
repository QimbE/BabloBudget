using BabloBudget.Api.Repository.Models;

namespace BabloBudget.Api.Repository.Extensions;

public static class MoneyFlowRepositoryExtensions
{
    public static IQueryable<MoneyFlowDto> GetOnTimeFlows(
        this IQueryable<MoneyFlowDto> moneyFlowRepository,
        DateOnly currentDateTime) =>
        moneyFlowRepository
            .GetNewFlows(currentDateTime)
            .Union(moneyFlowRepository.GetCurrentFlows(currentDateTime));
    
    private static IQueryable<MoneyFlowDto> GetNewFlows(
        this IQueryable<MoneyFlowDto> moneyFlowRepository,
        DateOnly currentDateTime) =>
        moneyFlowRepository
            .Where(mf => mf.LastCheckedUtc == null)
            .Where(mf => mf.StartingDateUtc <= currentDateTime);
    
    private static IQueryable<MoneyFlowDto> GetCurrentFlows(
        this IQueryable<MoneyFlowDto> moneyFlowRepository,
        DateOnly currentDateTime) =>
        moneyFlowRepository
            .Where(mf => mf.LastCheckedUtc != null)
            .Where(mf => mf.LastCheckedUtc!.Value.AddDays(mf.PeriodDays) <= currentDateTime);
}