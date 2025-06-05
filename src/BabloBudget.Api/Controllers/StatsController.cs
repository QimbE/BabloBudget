using BabloBudget.Api.Domain;
using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Models;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BabloBudget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class StatsController(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IDateTimeProvider dateTimeProvider)
    : ControllerBase
{
    [HttpPost("GetCurrentBalance")]
    public async Task<IActionResult> GetCurrentBalanceAsync(
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            var accountDto = await dbContext.Accounts
                .SingleOrDefaultAsync(a => a.Id == userId, token);

            if (accountDto is null)
                return BadRequest("Account does not exist");

            var balance = accountDto.BasisSum;

            var accountEntries = (await dbContext.AccountEntries
                .Where(ae => ae.AccountId == userId)
                .ToListAsync(token))
                .ToImmutableList();

            foreach (var accountEntry in accountEntries)
                balance += accountEntry.Sum;

            return Ok(balance);
        },
        cancellationToken: token);

        return result;
    }

    [HttpPost("GetBalanceAtDate")]
    public async Task<IActionResult> GetBalanceAtDateAsync(
        [FromQuery] DateOnly dateInclusive,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            var accountDto = await dbContext.Accounts
                .SingleOrDefaultAsync(a => a.Id == userId, token);

            if (accountDto is null)
                return BadRequest("Account does not exist");

            var balance = accountDto.BasisSum;

            var accountEntries = (await dbContext.AccountEntries
                .Where(ae => 
                    ae.AccountId == userId &&
                    ae.DateUtc <= dateInclusive)
                .ToListAsync(token))
                .ToImmutableList();

            foreach (var accountEntry in accountEntries)
                balance += accountEntry.Sum;

            return Ok(balance);
        },
        cancellationToken: token);

        return result;
    }

    [HttpPost("GetBalanceChangeInPeriodBySteps")]
    public async Task<IActionResult> GetBalanceChangeInPeriodByStepsAsync(
        [FromQuery] DateOnly startDateInclusive,
        [FromQuery] DateOnly endDateInclusive,
        [FromQuery] int steps,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            if (steps < 4 || steps > 12)
                return BadRequest("Invalid number of steps");

            int dayDiff = endDateInclusive.DayNumber - startDateInclusive.DayNumber + 1;

            if (startDateInclusive > dateTimeProvider.UtcNowDateOnly || 
                endDateInclusive > dateTimeProvider.UtcNowDateOnly ||
                dayDiff < steps)
                return BadRequest("Invalid date period");

            var accountDto = await dbContext.Accounts
                .SingleOrDefaultAsync(a => a.Id == userId, token);

            if (accountDto is null)
                return BadRequest("Account does not exist");

            List<decimal> sums = Enumerable.Repeat((decimal)0, steps).ToList();

            var accountEntries = (await dbContext.AccountEntries
                .Where(ae =>
                    ae.AccountId == userId &&
                    startDateInclusive <= ae.DateUtc && ae.DateUtc <= endDateInclusive)
                .ToListAsync(token))
                .ToImmutableList();

            double stepLength = (double)dayDiff / steps;
            foreach (var accountEntry in accountEntries)
            {
                int step = (int)Math.Floor((accountEntry.DateUtc.DayNumber - startDateInclusive.DayNumber) / stepLength);
                sums[step] += accountEntry.Sum;
            }   

            return Ok(sums);
        },
        cancellationToken: token);

        return result;
    }
}