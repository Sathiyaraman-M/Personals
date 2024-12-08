using Expensive.Common.Constants;
using Expensive.Server.Extensions;
using Expensive.Users.Abstractions.Utilities;
using Expensive.Users.Utilities;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using TimeProvider = Expensive.Infrastructure.Services.TimeProvider;

namespace Expensive.Tests.Base;

public class TestJwtBearerBuilder
{
    private static readonly IConfigurationRoot ConfigurationRoot =
        new ConfigurationBuilder().AddTraditionalFormatEnvironmentVariables().Build();

    private static readonly IJwtTokenConfiguration JwtTokenConfiguration = new JwtTokenConfiguration(ConfigurationRoot);

    private static JwtTokenHandler JwtTokenHandler { get; } = new(JwtTokenConfiguration, new TimeProvider());

    private List<Claim> Claims { get; } = [];

    private TestJwtBearerBuilder()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", TestConstants.JwtSecret);
    }

    public static TestJwtBearerBuilder Create()
    {
        return new TestJwtBearerBuilder();
    }

    public static TestJwtBearerBuilder CreateWithDefaultClaims()
    {
        return Create()
            .WithUserId("1")
            .WithLoginName("admin")
            .WithFullName("Admin")
            .WithEmail("bruce@wayne-enterprises.com")
            .WithPhone("1234567890");
    }

    public TestJwtBearerBuilder WithUserId(string userId) => WithClaim(ApplicationClaimTypes.UserId, userId);

    public TestJwtBearerBuilder WithLoginName(string loginName) =>
        WithClaim(ApplicationClaimTypes.LoginName, loginName);

    public TestJwtBearerBuilder WithFullName(string fullName) => WithClaim(ApplicationClaimTypes.FullName, fullName);

    public TestJwtBearerBuilder WithEmail(string email) => WithClaim(ApplicationClaimTypes.Email, email);

    public TestJwtBearerBuilder WithPhone(string phone) => WithClaim(ApplicationClaimTypes.Phone, phone);

    public TestJwtBearerBuilder WithPermission(string permission) =>
        WithClaim(ApplicationClaimTypes.Permission, permission);

    public TestJwtBearerBuilder WithPermissions(params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            WithPermission(permission);
        }

        return this;
    }

    public TestJwtBearerBuilder WithClaim(string type, string value)
    {
        if (Claims.Any(claim => claim.Type == type) && type != ApplicationClaimTypes.Permission)
        {
            Claims.RemoveAll(claim => claim.Type == type);
        }

        Claims.Add(new Claim(type, value));
        return this;
    }

    public string Build() => JwtTokenHandler.BuildToken(Claims);
}