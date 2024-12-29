using Personals.Common.Abstractions.Services;
using Personals.Users.Abstractions.Utilities;
using Personals.Users.Utilities;
using Personals.Tests.Base;
using Personals.Tests.Base.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Personals.Users.Tests.Utilties;

public class JwtTokenHandlerTests
{
    private readonly IJwtTokenConfiguration _configuration = Substitute.For<IJwtTokenConfiguration>();

    private ITimeProvider _timeProvider;

    public JwtTokenHandlerTests()
    {
        _configuration.Secret.Returns(TestConstants.JwtSecret);
        _configuration.Issuer.Returns("issuer");
        _configuration.Audience.Returns("audience");
        _configuration.TokenExpirationInMinutes.Returns(60);
        _configuration.RefreshTokenExpirationInMinutes.Returns(1440);
        _timeProvider = new StubTimeProvider();
    }

    [Fact]
    public void BuildToken_ShouldReturnToken()
    {
        // Arrange
        var jwtTokenBuilder = new JwtTokenHandler(_configuration, _timeProvider);
        var claims = new List<Claim> { new("claim1", "value1"), new("claim2", "value2") };

        // Act
        var token = jwtTokenBuilder.BuildToken(claims);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var jwtSecurityToken = ReadJwtSecurityTokenFromString(token);
        jwtSecurityToken.Should().NotBeNull();
        jwtSecurityToken!.Issuer.Should().Be("issuer");
        jwtSecurityToken.Audiences.Should().ContainSingle("audience");
        jwtSecurityToken.ValidTo.Should().BeCloseTo(_timeProvider.Now.AddMinutes(60), TimeSpan.FromSeconds(10));

        var claimsFromToken = jwtSecurityToken.Claims.ToList();
        claimsFromToken.Should().NotBeNullOrEmpty();
        claimsFromToken.Should().Contain(claim => claim.Type == "claim1" && claim.Value == "value1");
        claimsFromToken.Should().Contain(claim => claim.Type == "claim2" && claim.Value == "value2");
    }

    [Fact]
    public void ExtractClaimsPrincipal_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        _timeProvider = Substitute.For<ITimeProvider>();
        _timeProvider.Now.Returns(DateTime.Today.AddDays(1));
        var jwtTokenBuilder = new JwtTokenHandler(_configuration, _timeProvider);
        var claims = new List<Claim> { new("claim1", "value1"), new("claim2", "value2") };
        var token = jwtTokenBuilder.BuildToken(claims);

        // Act
        var principal = jwtTokenBuilder.ExtractClaimsPrincipal(token);

        // Assert
        principal.Should().NotBeNull();
        principal.Claims.Should().NotBeNullOrEmpty();
        principal.Claims.Should().Contain(claim => claim.Type == "claim1" && claim.Value == "value1");
        principal.Claims.Should().Contain(claim => claim.Type == "claim2" && claim.Value == "value2");
    }

    [Fact]
    public void ExtractClaimsPrincipal_WithInvalidAlgorithm_ShouldThrowSecurityTokenException()
    {
        // Arrange
        _timeProvider = Substitute.For<ITimeProvider>();
        _timeProvider.Now.Returns(DateTime.Today.AddDays(1));
        var jwtTokenBuilder = new JwtTokenHandler(_configuration, _timeProvider);
        var claims = new List<Claim> { new("claim1", "value1"), new("claim2", "value2") };
        var token = jwtTokenBuilder.BuildToken(claims);
        var jwtSecurityToken = ReadJwtSecurityTokenFromString(token)!;
        var invalidSigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestConstants.JwtSecret)),
            SecurityAlgorithms.HmacSha256Signature);
        var invalidToken = new JwtSecurityToken(jwtSecurityToken.Issuer,
            jwtSecurityToken.Audiences.First(),
            jwtSecurityToken.Claims,
            expires: jwtSecurityToken.ValidTo,
            signingCredentials: invalidSigningCredentials);

        // Act
        Action act = () =>
            jwtTokenBuilder.ExtractClaimsPrincipal(new JwtSecurityTokenHandler().WriteToken(invalidToken));

        // Assert
        act.Should().Throw<SecurityTokenException>().WithMessage("Invalid token");
    }

    private static JwtSecurityToken? ReadJwtSecurityTokenFromString(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadToken(token) as JwtSecurityToken;
    }
}