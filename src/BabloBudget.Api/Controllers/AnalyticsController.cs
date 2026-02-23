using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BabloBudget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class AnalyticsController(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IDateTimeProvider dateTimeProvider)
    : ControllerBase
{
    [HttpGet("GetSumsByPeriod")]
    public async Task<IActionResult> GetSumsByPeriodAsync(
        [FromQuery] TimeGrouping periodType,
        [FromQuery] int periodsCount,
        [FromQuery] FlowType flowType,
        [FromQuery] Guid? categoryId,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        if (periodsCount <= 0)
            return BadRequest("Periods count must be positive");

        return await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            if (categoryId is not null)
            {
                var categoryDto = await dbContext.Categories
                    .SingleOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, token);

                if (categoryDto is null)
                    return BadRequest($"Category with id {categoryId} does not exist for this user");
            }

            var today = dateTimeProvider.UtcNowDateOnly;
            var periods = GeneratePeriods(today, periodType, periodsCount);

            var startDate = periods[^1].Date;
            var endDate = periods[0].Date;
            if (periodType == TimeGrouping.Month)
                endDate = endDate.AddMonths(1).AddDays(-1);

            var query = flowType == FlowType.Expense
                ? dbContext.AccountEntries.Where(ae => ae.AccountId == userId && ae.Sum < 0)
                : dbContext.AccountEntries.Where(ae => ae.AccountId == userId && ae.Sum > 0);

            if (categoryId is not null)
                query = query.Where(ae => ae.CategoryId == categoryId);

            query = query.Where(ae => ae.DateUtc >= startDate && ae.DateUtc <= endDate);

            var entries = await query.ToListAsync(token);

            foreach (var entry in entries)
            {
                var periodIndex = GetPeriodIndex(entry.DateUtc, today, periodType);
                if (periodIndex >= 0 && periodIndex < periodsCount)
                {
                    periods[periodIndex] = periods[periodIndex] with
                    {
                        Total = periods[periodIndex].Total + Math.Abs(entry.Sum)
                    };
                }
            }

            return Ok(periods);
        },
        cancellationToken: token);
    }

    [HttpGet("GetCategoryPercentages")]
    public async Task<IActionResult> GetCategoryPercentagesAsync(
        [FromQuery] TimeGrouping periodType,
        [FromQuery] int periodsCount,
        [FromQuery] FlowType flowType,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        if (periodsCount <= 0)
            return BadRequest("Periods count must be positive");

        return await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var today = dateTimeProvider.UtcNowDateOnly;
            var periods = GeneratePeriods(today, periodType, periodsCount);

            var startDate = periods[^1].Date;
            var endDate = periods[0].Date;
            if (periodType == TimeGrouping.Month)
                endDate = endDate.AddMonths(1).AddDays(-1);

            var query = flowType == FlowType.Expense
                ? dbContext.AccountEntries.Where(ae => ae.AccountId == userId && ae.Sum < 0)
                : dbContext.AccountEntries.Where(ae => ae.AccountId == userId && ae.Sum > 0);

            query = query.Where(ae => ae.DateUtc >= startDate && ae.DateUtc <= endDate);

            var entriesWithCategories = await (
                from entry in query
                join category in dbContext.Categories on entry.CategoryId equals category.Id into categoryJoin
                from category in categoryJoin.DefaultIfEmpty()
                select new { Entry = entry, CategoryName = category != null ? category.Name : "Без категории" }
            ).ToListAsync(token);

            if (!entriesWithCategories.Any())
                return Ok(Array.Empty<CategoryPercentageResult>());

            var totalSum = entriesWithCategories.Sum(e => Math.Abs(e.Entry.Sum));

            if (totalSum == 0)
                return Ok(Array.Empty<CategoryPercentageResult>());

            var categoryGroups = entriesWithCategories
                .GroupBy(e => e.CategoryName)
                .Select(g => new CategoryPercentageResult(
                    g.Key,
                    Math.Round((g.Sum(e => Math.Abs(e.Entry.Sum)) / totalSum) * 100, 2)))
                .OrderByDescending(r => r.Percentage)
                .ToList();

            return Ok(categoryGroups);
        },
        cancellationToken: token);
    }

    private static List<PeriodSumResult> GeneratePeriods(DateOnly today, TimeGrouping periodType, int count)
    {
        var result = new List<PeriodSumResult>(count);

        for (int i = 0; i < count; i++)
        {
            var date = periodType switch
            {
                TimeGrouping.Day => today.AddDays(-i),
                TimeGrouping.Month => new DateOnly(today.Year, today.Month, 1).AddMonths(-i),
                _ => throw new ArgumentOutOfRangeException(nameof(periodType))
            };

            result.Add(new PeriodSumResult(date, 0));
        }

        return result;
    }

    private static int GetPeriodIndex(DateOnly entryDate, DateOnly today, TimeGrouping periodType)
    {
        return periodType switch
        {
            TimeGrouping.Day => today.DayNumber - entryDate.DayNumber,
            TimeGrouping.Month => (today.Year - entryDate.Year) * 12 + (today.Month - entryDate.Month),
            _ => throw new ArgumentOutOfRangeException(nameof(periodType))
        };
    }
}

public enum TimeGrouping
{
    Day = 0,
    Month = 1
}

public enum FlowType
{
    Expense = 0,
    Income = 1
}

public sealed record PeriodSumResult(
    [property: JsonPropertyName("date")]
    DateOnly Date,

    [property: JsonPropertyName("total")]
    decimal Total);

public sealed record CategoryPercentageResult(
    [property: JsonPropertyName("categoryName")]
    string CategoryName,

    [property: JsonPropertyName("percentage")]
    decimal Percentage);