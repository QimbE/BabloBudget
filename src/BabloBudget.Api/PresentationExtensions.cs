﻿using BabloBudget.Api.Repository;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

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

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowAnyOrigin();
            });
        });

        services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new() { Title="BabloBudget API", Version="v1" });
            options.AddSecurityDefinition("oauth2", new()
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        services.AddAuthorization();

        services.AddAuthentication(c => c.DefaultAuthenticateScheme = IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);
        
        return services;
    }
    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
        
        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<UserStore<IdentityUser<Guid>, IdentityRole<Guid>, ApplicationDbContext, Guid>>()
            .AddRoleStore<RoleStore<IdentityRole<Guid>, ApplicationDbContext, Guid>>()
            .AddApiEndpoints();

        services.AddTransient<IDateTimeProvider, DefaultDateTimeProvider>();
        
        return services;
    }
}