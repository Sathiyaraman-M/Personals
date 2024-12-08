using Expensive.Common.Constants;
using Expensive.Common.Contracts.Users;
using Expensive.Infrastructure.Permissions;
using Expensive.Users.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expensive.Users.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Permission(Permissions.Users.View)]
    public async Task<IActionResult> GetAllUsersAsync(int page = 1, int pageSize = 10, string? searchText = null)
    {
        return Ok(await userService.GetAllAsync(page, pageSize, searchText));
    }

    [HttpGet("{id:guid}")]
    [Permission(Permissions.Users.View)]
    public async Task<IActionResult> GetUserAsync(Guid id)
    {
        return Ok(await userService.GetByIdAsync(id));
    }

    [HttpGet("{id:guid}/permissions")]
    [Permission(Permissions.Users.ViewPermissions)]
    public async Task<IActionResult> GetUserPermissionsAsync(Guid id)
    {
        return Ok(await userService.GetPermissionsAsync(id));
    }

    [HttpPost]
    [Permission(Permissions.Users.Create)]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest userRequest)
    {
        var result = await userService.CreateAsync(userRequest);
        return Created("/api/users", result);
    }

    [HttpPut("{id:guid}/permissions")]
    [Permission(Permissions.Users.UpdatePermissions)]
    public async Task<IActionResult> UpdateUserPermissionsAsync(Guid id,
        UpdateUserPermissionsRequest permissionsRequest)
    {
        return Ok(await userService.UpdatePermissionsAsync(id, permissionsRequest));
    }

    [HttpPut("{id:guid}")]
    [Permission(Permissions.Users.Update)]
    public async Task<IActionResult> UpdateUserAsync(Guid id, UpdateUserRequest userRequest)
    {
        return Ok(await userService.UpdateAsync(id, userRequest));
    }

    [HttpDelete("{id:guid}")]
    [Permission(Permissions.Users.Delete)]
    public async Task<IActionResult> DeleteUserAsync(Guid id)
    {
        await userService.DeleteAsync(id);
        return NoContent();
    }
}