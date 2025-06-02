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
    [HttpPost("GetCurrentBallance")]
    public async Task<IActionResult> GetCurrentBallanceAsync(
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

            var ballance = accountDto.BasisSum;

            var accountEntries = (await dbContext.AccountEntries
                .Where(ae => ae.AccountId == userId)
                .ToListAsync(token))
                .ToImmutableList();

            foreach (var accountEntry in accountEntries)
                ballance += accountEntry.Sum;

            return Ok(ballance);
        },
        cancellationToken: token);

        return result;
    }
}