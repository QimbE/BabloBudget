using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Models;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BabloBudget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class AccountController(IDbContextFactory<ApplicationDbContext> dbContextFactory) : ControllerBase
{
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccountAsync(
        [FromQuery]decimal basisSum,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();
        
        if (userId is null)
            return BadRequest("Unable to identify user");
        
        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            var isUserExists = await dbContext.Users.AnyAsync(u => u.Id == userId, token);
            if (!isUserExists)
            {
                return NotFound("User not found");
            }
            var isAccountExists = await dbContext.Accounts.AnyAsync(a => a.Id == userId, token);
            if (isAccountExists)
            {
                return BadRequest("Account already exists");
            }
            var money = Money.Create(basisSum);
            var account = Account.Create(money, userId.Value);
            var accountDto = AccountDto.FromDomainModel(account);
            dbContext.Accounts.Add(accountDto);
            await dbContext.SaveChangesAsync(token);
            return Ok("Account created successfully");
        },
        cancellationToken: token);

        return result;
    }

    [HttpGet("GetBasisSum")]
    public async Task<IActionResult> GetBasisSumAsync(CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();
        
        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            var isUserExists = await dbContext.Users.AnyAsync(u => u.Id == userId, token);
            if (!isUserExists)
            {
                return NotFound("User not found");
            }
            var userAccount = await dbContext.Accounts.SingleOrDefaultAsync(a => a.Id == userId, token);
            if (userAccount is null)
            {
                return NotFound("Account not found");
            }
            return Ok(userAccount.BasisSum);
        },
        cancellationToken: token);

        return result;
    }

    [HttpPut("Update")]
    public async Task<IActionResult> UpdateAccountAsync(
        [FromQuery] decimal newBasisSum,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();
        
        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            var isUserExists = await dbContext.Users.AnyAsync(u => u.Id == userId, token);
            if (!isUserExists)
            {
                return NotFound("User not found");
            }
            var userAccount = await dbContext.Accounts.SingleOrDefaultAsync(a => a.Id == userId, token);
            if (userAccount is null)
            {
                return NotFound("Account not found");
            }
            var account = userAccount.ToDomainModel();
            var money = Money.Create(newBasisSum);
            var newAccount = account with { BasisSum = money };
            var newAccountDto = AccountDto.FromDomainModel(newAccount);
            dbContext.Accounts.Update(newAccountDto);
            await dbContext.SaveChangesAsync(token);
            return Ok("Account updated successfully");
        },
        cancellationToken: token);

        return result;
    }
}
