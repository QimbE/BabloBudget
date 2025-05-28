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

    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteAccountEntryAsync(
        [FromQuery] Guid accountEntryId,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            var accountEntryDto = await dbContext.AccountEntries
                .SingleOrDefaultAsync(a => a.Id == accountEntryId, token);

            if (accountEntryDto is null)
                return NotFound("Account entry does not exist");

            if (accountEntryDto.AccountId != userId)
                return Forbid();

            dbContext.AccountEntries.Remove(accountEntryDto);

            await dbContext.SaveChangesAsync(token);

            return Ok();
        },
        cancellationToken: token);

        return result;
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetAccountEntryAsync(
        [FromQuery] Guid accountEntryId,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var accountEntryDto = await dbContext.AccountEntries
                .SingleOrDefaultAsync(a => a.Id == accountEntryId, token);

            if (accountEntryDto is null)
                return NotFound("Account entry does not exist");

            if (accountEntryDto.AccountId != userId)
                return Forbid();

            return Ok(accountEntryDto);
        },
        cancellationToken: token);

        return result;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllAccountEntriesAsync(CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var accountEntries = (await dbContext.AccountEntries
                .Where(ae => ae.AccountId == userId)
                .ToListAsync(token))
                .ToImmutableList();

            return Ok(accountEntries);
        },
        cancellationToken: token);

        return result;
    }

    [HttpGet("GetInDateSpan")]
    public async Task<IActionResult> GetAccountEntriesInDateSpanAsync(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        if (startDate > endDate)
            return BadRequest("Invalid date span");

        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var accountEntries = (await dbContext.AccountEntries
                .Where(ae => 
                    ae.AccountId == userId && 
                    startDate <= ae.DateUtc && ae.DateUtc <= endDate)
                .ToListAsync(token))
                .ToImmutableList();

            return Ok(accountEntries);
        },
        cancellationToken: token);

        return result;
    }

    [HttpGet("GetAllByCategory")]
    public async Task<IActionResult> GetAllAccountEntriesByCategoryAsync(
        [FromQuery] Guid? categoryId, 
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var categoryDto = categoryId is null
                ? null
                : await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId, token);

            if (categoryDto is null != categoryId is null)
                return BadRequest($"Category with id {categoryId} does not exist");

            var accountEntries = (await dbContext.AccountEntries
                .Where(ae => 
                    ae.AccountId == userId &&
                    ae.CategoryId == categoryId)
                .ToListAsync(token))
                .ToImmutableList();

            return Ok(accountEntries);
        },
        cancellationToken: token);

        return result;
    }

    [HttpGet("GetByCategoryInDateSpan")]
    public async Task<IActionResult> GetAccountEntriesByCategoriesInDateSpanAsync(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? categoryId,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        if (startDate > endDate)
            return BadRequest("Invalid date span");

        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var categoryDto = categoryId is null
                ? null
                : await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId, token);

            if (categoryDto is null != categoryId is null)
                return BadRequest($"Category with id {categoryId} does not exist");

            var accountEntries = (await dbContext.AccountEntries
                .Where(ae =>
                    ae.AccountId == userId &&
                    startDate <= ae.DateUtc && ae.DateUtc <= endDate &&
                    ae.CategoryId == categoryId)
                .ToListAsync(token))
                .ToImmutableList();

            return Ok(accountEntries);
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