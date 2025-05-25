using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.Serialization;
using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Models;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using BabloBudget.Api.Domain;

namespace BabloBudget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class MoneyFlowController(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IDateTimeProvider dateTimeProvider)
    : ControllerBase
{
    [HttpPost("Create")]
    public async Task<IActionResult> CreateMoneyFlowAsync(
        [FromBody] CreateMoneyFlowRequest createMoneyFlowRequest,
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
            
            var (sum, startingDate, period, categoryId) = createMoneyFlowRequest;
            var categoryDto = categoryId is null
                ? null
                : await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == createMoneyFlowRequest.CategoryId, token);
            
            if (categoryDto is null != categoryId is null)
                return BadRequest($"Category with id {categoryId} does not exist");

            var account = accountDto.ToDomainModel();
            var category = categoryDto?.ToDomainModel();

            var transaction = Transaction.Create(Money.Create(sum), category);
            var schedule = PeriodicalSchedule.New(
                startingDate,
                Period.FromDays(period.ToDays()),
                dateTimeProvider);
            
            var moneyFlow = MoneyFlow.Create(Guid.NewGuid(), account, transaction, schedule);
            
            var moneyFlowResponseDto = MoneyFlowDto.FromDomainModel(moneyFlow);

            dbContext.MoneyFlows.Add(moneyFlowResponseDto);
            await dbContext.SaveChangesAsync(token);
            return Ok(moneyFlowResponseDto);
        },
        cancellationToken: token);

        return result;
    }
    
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteMoneyFlowAsync(
        [FromQuery] Guid moneyFlowId,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();
        
        if (userId is null)
            return BadRequest("Unable to identify user");
        
        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
            {
                var moneyFlowDto = await dbContext.MoneyFlows
                    .SingleOrDefaultAsync(a => a.Id == moneyFlowId, token);
            
                if (moneyFlowDto is null)
                    return NotFound("Money flow does not exist");
                
                if (moneyFlowDto.AccountId != userId)
                    return Forbid();
                
                dbContext.MoneyFlows.Remove(moneyFlowDto);
                
                await dbContext.SaveChangesAsync(token);

                return Ok();
            },
            cancellationToken: token);

        return result;
    }
    
    [HttpGet("GetById")]
    public async Task<IActionResult> GetMoneyFlowAsync(
        [FromQuery] Guid moneyFlowId,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();
        
        if (userId is null)
            return BadRequest("Unable to identify user");
        
        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
            {
                var moneyFlowDto = await dbContext.MoneyFlows
                    .SingleOrDefaultAsync(a => a.Id == moneyFlowId, token);
            
                if (moneyFlowDto is null)
                    return NotFound("Money flow does not exist");
                
                if (moneyFlowDto.AccountId != userId)
                    return Forbid();

                return Ok(moneyFlowDto);
            },
            cancellationToken: token);

        return result;
    }
    
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllMoneyFlowsAsync(CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();
        
        if (userId is null)
            return BadRequest("Unable to identify user");
        
        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
            {
                var moneyFlows = (await dbContext.MoneyFlows
                    .Where(f => f.AccountId == userId)
                    .ToListAsync(token))
                    .ToImmutableList();

                return Ok(moneyFlows);
            },
            cancellationToken: token);

        return result;
    }
    
    [HttpPut("Update")]
    public async Task<IActionResult> UpdateMoneyFlowAsync(
        [FromBody] CreateMoneyFlowRequest createMoneyFlowRequest,
        [FromQuery] Guid moneyFlowId,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();
        
        if (userId is null)
            return BadRequest("Unable to identify user");
        
        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
            {
                var sourceMoneyFlowDto = await dbContext.MoneyFlows
                    .SingleOrDefaultAsync(a => a.Id == moneyFlowId, token);
                
                if (sourceMoneyFlowDto is null)
                    return NotFound("Money flow does not exist");
                
                if (sourceMoneyFlowDto.AccountId != userId)
                    return Forbid();
                
                var accountDto = await dbContext.Accounts
                    .SingleOrDefaultAsync(a => a.Id == userId, token);
            
                if (accountDto is null)
                    return BadRequest("Account does not exist");
            
                var (sum, startingDate, period, categoryId) = createMoneyFlowRequest;
                var categoryDto = categoryId is null
                    ? null
                    : await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == createMoneyFlowRequest.CategoryId, token);
            
                if (categoryDto is null != categoryId is null)
                    return BadRequest($"Category with id {categoryId} does not exist");

                var account = accountDto.ToDomainModel();
                var category = categoryDto?.ToDomainModel();

                var moneyFlowDto = new MoneyFlowDto()
                {
                    Id = moneyFlowId,
                    AccountId = userId.Value,
                    CategoryId = categoryId,
                    StartingDateUtc = startingDate,
                    LastCheckedUtc = null,
                    PeriodDays = period.ToDays(),
                    Sum = sum,
                };
                var moneyFlow = moneyFlowDto.ToDomainModel(account, category, dateTimeProvider);
            
                var moneyFlowResponseDto = MoneyFlowDto.FromDomainModel(moneyFlow);

                dbContext.MoneyFlows.Update(moneyFlowResponseDto);
                await dbContext.SaveChangesAsync(token);
                return Ok(moneyFlowResponseDto);
            },
            cancellationToken: token);

        return result;
    }
}

public enum PeriodType
{
    Daily,
    Monthly,
    Yearly
}

public static class PeriodTypeExtensions
{
    public static int ToDays(this PeriodType periodType) => periodType switch
    {
        PeriodType.Daily => 1,
        PeriodType.Monthly => 30,
        PeriodType.Yearly => 365,
        _ => throw new UnreachableException("Unexpected enum value")
    };
}

public sealed record CreateMoneyFlowRequest(
    
    [property: JsonPropertyName("sum")]
    decimal Sum,
    
    [property: JsonPropertyName("startingDate")]
    DateOnly StartingDate,
    
    [property: JsonPropertyName("period"), JsonConverter(typeof(JsonStringEnumConverter))]
    PeriodType Period,
    
    [property: JsonPropertyName("categoryId")]
    Guid? CategoryId);
