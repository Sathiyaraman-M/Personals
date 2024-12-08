using Expensive.Common.Contracts.Users;
using Expensive.Common.Wrappers;
using Expensive.Common.Wrappers.Abstractions;
using Expensive.UI.Abstractions.Services.Http;
using Expensive.UI.Extensions;
using System.Net.Http.Json;

namespace Expensive.UI.Services.Http;

public class UserService(HttpClient httpClient) : IUserService
{
    public async Task<PaginatedResult<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, string searchText, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<PaginatedResult<UserResponse>>($"api/users?page={pageNumber}&pageSize={pageSize}&searchText={searchText}", cancellationToken);
        return response!;
    }

    public async Task<IResult<UserResponse>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/users/{id}", cancellationToken);
        return await response.ToResult<UserResponse>(cancellationToken);
    }

    public async Task<IResult<List<string>>> GetUserPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/users/{id}/permissions", cancellationToken);
        return await response.ToResult<List<string>>(cancellationToken);
    }

    public async Task<IResult> CreateUserAsync(CreateUserRequest model)
    {
        var response = await httpClient.PostAsJsonAsync("api/users", model);
        response.EnsureSuccessStatusCode();
        return await response.ToResult();
    }

    public async Task<IResult> UpdateUserPermissionsAsync(Guid id, UpdateUserPermissionsRequest model)
    {
        var response = await httpClient.PutAsJsonAsync($"api/users/{id}/permissions", model);
        response.EnsureSuccessStatusCode();
        return await response.ToResult();
    }

    public async Task<IResult> UpdateUserAsync(Guid id, UpdateUserRequest model)
    {
        var response = await httpClient.PutAsJsonAsync($"api/users/{id}", model);
        response.EnsureSuccessStatusCode();
        return await response.ToResult();
    }

    public async Task<IResult> DeleteUserAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/users/{id}");
        if (response.IsSuccessStatusCode)
        {
            return SuccessfulResult.Succeed("User deleted successfully.");
        }
        return await response.ToResult();
    }
}