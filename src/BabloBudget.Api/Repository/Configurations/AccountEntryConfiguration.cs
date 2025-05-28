using BabloBudget.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BabloBudget.Api.Repository.Configurations;

public class AccountEntryConfiguration : IEntityTypeConfiguration<AccountEntryDto>
{
    public void Configure(EntityTypeBuilder<AccountEntryDto> builder)
    {
        builder.ToTable("AccountEntries");

        builder.HasKey(ae => ae.Id);

        builder
            .Property(ae => ae.DateUtc)
            .IsRequired()
            .HasConversion<DateOnly>();

        builder
            .Property(ae => ae.Sum)
            .IsRequired()
            .HasConversion<decimal>();

        builder
            .HasOne<CategoryDto>()
            .WithMany()
            .HasForeignKey(ae => ae.CategoryId)
            .IsRequired(false);

        builder
            .HasOne<AccountDto>()
            .WithMany()
            .HasForeignKey(ae => ae.AccountId)
            .IsRequired();

        builder
            .HasIndex(ae => new { ae.AccountId, ae.DateUtc, ae.Sum });
    }
}
