using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.Serialization;
using BabloBudget.Api.Repository;
using BabloBudget.Api.Repository.Models;
using BabloBudget.Api.Repository.Resilience;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using BabloBudget.Api.Domain;

namespace BabloBudget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class CategoryController(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IDateTimeProvider dateTimeProvider)
    : ControllerBase
{
    [HttpPost("Create")]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CreateCategoryRequest createCategoryRequest,
        CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteAndCommitAsync<IActionResult>(async dbContext =>
        {
            var (name, type) = createCategoryRequest;

            var category = Category.Create(Guid.NewGuid(), (Guid)userId, name, type);

            var categoryResponseDto = CategoryDto.FromDomainModel(category);

            dbContext.Categories.Add(categoryResponseDto);
            await dbContext.SaveChangesAsync(token);
            return Ok(categoryResponseDto);
        },
        cancellationToken: token);

        return result;
    }

    [HttpGet("GetIncome")]
    public async Task<IActionResult> GetIncomeCategoriesAsync(CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var categories = (await dbContext.Categories
                .Where(c => 
                    c.UserId == userId &&
                    c.Type == CategoryType.Income)
                .ToListAsync(token))
                .ToImmutableList();

            return Ok(categories);
        },
            cancellationToken: token);

        return result;
    }

    [HttpGet("GetExpense")]
    public async Task<IActionResult> GetExpenseCategoriesAsync(CancellationToken token)
    {
        var userId = HttpContext.User.TryParseUserId();

        if (userId is null)
            return BadRequest("Unable to identify user");

        var result = await dbContextFactory.ExecuteReadonlyAsync<IActionResult>(async dbContext =>
        {
            var categories = (await dbContext.Categories
                .Where(c =>
                    c.UserId == userId &&
                    c.Type == CategoryType.Expense)
                .ToListAsync(token))
                .ToImmutableList();

            return Ok(categories);
        },
            cancellationToken: token);

        return result;
    }
}

public sealed record CreateCategoryRequest(

    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))]
    CategoryType Type);
