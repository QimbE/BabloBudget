using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace BabloBudget.Api.Repository;

public sealed class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    
}