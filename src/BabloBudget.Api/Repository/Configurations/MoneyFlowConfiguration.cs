using BabloBudget.Api.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BabloBudget.Api.Repository.Configurations;

public class MoneyFlowConfiguration : IEntityTypeConfiguration<MoneyFlowDto>
{
    public void Configure(EntityTypeBuilder<MoneyFlowDto> builder)
    {
        builder.ToTable("MoneyFlows");

        builder.HasKey(a => a.Id);

        builder
            .HasOne<AccountDto>()
            .WithMany()
            .HasForeignKey(mf => mf.AccountId)
            .IsRequired();
        
        builder
            .HasOne<CategoryDto>()
            .WithMany()
            .HasForeignKey(mf => mf.CategoryId)
            .IsRequired(false);

        builder
            .Property(mf => mf.Sum)
            .IsRequired()
            .HasConversion<decimal>();

        builder
            .Property(mf => mf.StartingDateUtc)
            .IsRequired()
            .HasConversion<DateOnly>();
        
        builder
            .Property(mf => mf.LastCheckedUtc)
            .IsRequired(false)
            .HasConversion<DateOnly?>();

        builder
            .Property(mf => mf.PeriodDays)
            .IsRequired()
            .HasConversion<int>();
    }
}
