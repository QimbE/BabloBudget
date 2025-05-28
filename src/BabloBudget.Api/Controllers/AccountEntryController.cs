using BabloBudget.Api.Domain;
using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Models;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BabloBudget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class AccountEntryController(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IDateTimeProvider dateTimeProvider) 
    : ControllerBase
{
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccountEntryAsync(
        [FromBody] CreateAccountEntryRequest createAccountEntryRequest,
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

            var (sum, date, categoryId) = createAccountEntryRequest;
            var categoryDto = categoryId is null
                ? null
                : await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId, token);

            if (categoryDto is null != categoryId is null)
                return BadRequest($"Category with id {categoryId} does not exist");

            var account = accountDto.ToDomainModel();
            var category = categoryDto?.ToDomainModel();

            var transaction = Transaction.Create(Money.Create(sum), category);

            var accountEntry = AccountEntry.Create(Guid.NewGuid(), date, transaction, account, dateTimeProvider);

            var accountEntryResponseDto = AccountEntryDto.FromDomainModel(accountEntry);

            dbContext.AccountEntries.Add(accountEntryResponseDto);
            await dbContext.SaveChangesAsync(token);
            return Ok(accountEntryResponseDto);
        },
        cancellationToken: token);

        return result;
    }
}

public sealed record CreateAccountEntryRequest(
    [property: JsonPropertyName("sum")]
    decimal Sum,

    [property: JsonPropertyName("date")]
    DateOnly Date,

    [property: JsonPropertyName("categoryId")]
    Guid? CategoryId);