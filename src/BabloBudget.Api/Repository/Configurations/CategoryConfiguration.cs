using BabloBudget.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BabloBudget.Api.Repository.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryDto>
{
    public void Configure(EntityTypeBuilder<CategoryDto> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        
        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>(
                ct => ct.ToString(),
                str => Enum.Parse<CategoryType>(str))
            .HasMaxLength(20);
    }
}