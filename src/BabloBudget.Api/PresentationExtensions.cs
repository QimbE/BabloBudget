using BabloBudget.Api.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                        TokenUrl = new Uri("https://www.googleapis.com/oauth2/v4/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect scope" },
                            { "profile", "Access your profile data" },
                            { "email", "Access your email address" }
                        }
                    }
                }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "openid", "profile", "email" }
                }
            });
        });
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                var googleAuthNSection = 
                    configuration.GetSection("Authentication:Google");

                options.ClientId = googleAuthNSection["ClientId"] ?? throw new NullReferenceException();
                options.ClientSecret = googleAuthNSection["ClientSecret"] ?? throw new NullReferenceException();
            });
        
        return services;
    }
    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        return services;
    }
}