using BabloBudget.Api.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BabloBudget.Api.Repository.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<AccountDto>
{
    public void Configure(EntityTypeBuilder<AccountDto> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder
            .HasOne<IdentityUser<Guid>>()
            .WithOne()
            .HasForeignKey<AccountDto>(a => a.Id);

        builder.Property(a => a.BasisSum)
            .IsRequired()
            .HasConversion<decimal>();
    }
}
