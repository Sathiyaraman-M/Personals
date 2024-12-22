using Personals.Common.Contracts.Links;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;
using Personals.UI.Abstractions.Services.Http;
using Personals.UI.Extensions;
using System.Net.Http.Json;

namespace Personals.UI.Services.Http;

public class LinkService(HttpClient httpClient) : ILinkService
{
    public async Task<PaginatedResult<LinkResponse>> GetAllAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/links?page={page}&pageSize={pageSize}&search={search}", cancellationToken);
        return await response.ToPaginatedResult<LinkResponse>(cancellationToken);
    }

    public async Task<IResult<LinkResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/links/{id}", cancellationToken);
        return await response.ToResult<LinkResponse>(cancellationToken);
    }

    public async Task<IResult> CreateAsync(CreateLinkRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/links", request);
        return await response.ToResult();
    }

    public async Task<IResult> UpdateAsync(Guid id, UpdateLinkRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/links/{id}", request);
        return await response.ToResult();
    }

    public async Task<IResult> DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/links/{id}");
        return await response.ToResult();
    }
}