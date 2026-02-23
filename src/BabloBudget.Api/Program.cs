using BabloBudget.Api;
using BabloBudget.Api.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddLayers(configuration);

var app = builder.Build();

app.UseCors("AllowAll");
app.ApplyMigrations();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<IdentityUser<Guid>>();

app.Run();


public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        
        dbContext.Database.Migrate();
        scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>().LogInformation("Applied migrations");
    }
}
