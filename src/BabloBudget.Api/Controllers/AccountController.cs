using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabloBudget.Api.Controllers;

[ApiController]
[Route("Account")]
public class AccountController(IDbContextFactory<ApplicationDbContext> dbContextFactory) : Controller
{

    // Should get user id from jwt
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var account = await dbContextFactory.ExecuteReadonlyAsync(
            async dbContext =>
            {
                // Вынести бы стратегию по Id и её тестить как бизнес логику
                var account = await dbContext.Accounts.SingleOrDefaultAsync(a => a.UserId == id, cancellationToken);
                
                return account;
            },
            cancellationToken: cancellationToken);
        
        return account is not null ? Ok(account) : NotFound();
    }
}