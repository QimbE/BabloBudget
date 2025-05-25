using BabloBudget.Api.Repository.Configurations;
using BabloBudget.Api.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BabloBudget.Api.Repository;

public sealed class ApplicationDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext (DbContextOptions<ApplicationDbContext > options)
        : base(options)
    {
    }
    
    public DbSet<AccountDto> Accounts { get; private set; }
    
    public DbSet<MoneyFlowDto> MoneyFlows { get; private set; }
    
    public DbSet<CategoryDto> Categories { get; private set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new AccountConfiguration());
        builder.ApplyConfiguration(new MoneyFlowConfiguration());
        builder.ApplyConfiguration(new CategoryConfiguration());
    }
}