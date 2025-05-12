namespace BabloBudget.Api.Repository.Models;

public sealed class AccountDto
{
    public required Guid Id { get; init; }
    
    public required decimal BasisSum { get; init; }
}