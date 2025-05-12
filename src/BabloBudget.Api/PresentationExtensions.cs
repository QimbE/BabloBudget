using BabloBudget.Api.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.MicrosoftExtensions;
using Microsoft.OpenApi.Models;

namespace BabloBudget.Api;

public static class PresentationExtensions
{
    public static IServiceCollection AddLayers(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddInfrastructure(configuration)
            .AddPresentation(configuration);

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new() { Title="BabloBudget API", Version="v1" });
        });

        services.AddAuthorization();

        services.AddAuthentication()
            //.AddCookie(IdentityConstants.ApplicationScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);
        
        return services;
    }
    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();
        
        return services;
    }
}