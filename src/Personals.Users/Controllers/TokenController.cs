using Personals.Common.Contracts.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personals.Users.Abstractions.Services;

namespace Personals.Users.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/token")]
public class TokenController(ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(TokenRequest tokenRequest)
    {
        var result = await tokenService.LoginAsync(tokenRequest);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        var result = await tokenService.RefreshTokenAsync(refreshTokenRequest);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}