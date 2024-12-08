using Expensive.Infrastructure.Abstractions.Utilities;
using Expensive.Users.Abstractions.Services;
using Expensive.Users.Abstractions.Utilities;
using Expensive.Users.Services;
using Expensive.Users.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Expensive.Users.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersModule<TJwtTokenConfiguration>(this IServiceCollection services)
        where TJwtTokenConfiguration : class, IJwtTokenConfiguration
    {
        services.AddUserService();
        services.AddTokenService<TJwtTokenConfiguration>();
        services.AddJwtAuthentication();
        return services;
    }

    private static void AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }

    private static void AddTokenService<TJwtTokenConfiguration>(this IServiceCollection services)
        where TJwtTokenConfiguration : class, IJwtTokenConfiguration
    {
        services.AddScoped<IJwtTokenHandler, JwtTokenHandler>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IJwtTokenConfiguration, TJwtTokenConfiguration>();
    }

    private static void AddJwtAuthentication(this IServiceCollection services)
    {
        var jwtTokenProvider = services.BuildServiceProvider().GetRequiredService<IJwtTokenConfiguration>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtTokenProvider.Issuer,
                    ValidAudience = jwtTokenProvider.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenProvider.Secret))
                };
            });
    }
}