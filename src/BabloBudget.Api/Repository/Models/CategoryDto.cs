namespace BabloBudget.Api.Repository.Models;

public sealed class CategoryDto
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required CategoryType Type { get; init; }

    public static CategoryDto FromDomainModel(Category domainModel) =>
        new()
        {
            Id = domainModel.Id,
            Name = domainModel.Name,
            Type = domainModel.Type
        };

    public Category ToDomainModel() =>
        Category.Create(Id, Name, Type);
}