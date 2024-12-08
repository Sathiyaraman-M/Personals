using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Enums;
using Expensive.Common.Wrappers;
using Expensive.Common.Wrappers.Abstractions;
using Expensive.UI.Abstractions.Services.Http;
using Expensive.UI.Extensions;
using System.Net.Http.Json;

namespace Expensive.UI.Services.Http;

public class LookupTypeService(HttpClient httpClient) : ILookupTypeService
{
    public async Task<PaginatedResult<LookupTypeResponse>> GetAllAsync(LookupTypeCategory category, int page,
        int pageSize, string? searchText,
        CancellationToken cancellationToken = default)
    {
        var baseUri = category switch
        {
            LookupTypeCategory.PaymentMethod => "payment-methods",
            LookupTypeCategory.ExpenseType => "expense-types",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
        var response = await httpClient.GetFromJsonAsync<PaginatedResult<LookupTypeResponse>>(
            $"api/lookup-types/{baseUri}?page={page}&pageSize={pageSize}&searchText={searchText}", cancellationToken);
        return response!;
    }

    public async Task<IResult<LookupTypeResponse>> GetLookupTypeByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/lookup-types/{id}", cancellationToken);
        return await response.ToResult<LookupTypeResponse>(cancellationToken);
    }

    public async Task<IResult> CreateLookupTypeAsync(CreateLookupTypeRequest model)
    {
        var response = await httpClient.PostAsJsonAsync("api/lookup-types", model);
        response.EnsureSuccessStatusCode();
        return await response.ToResult();
    }

    public async Task<IResult> UpdateLookupTypeAsync(Guid id, UpdateLookupTypeRequest model)
    {
        var response = await httpClient.PutAsJsonAsync($"api/lookup-types/{id}", model);
        response.EnsureSuccessStatusCode();
        return await response.ToResult();
    }

    public async Task<IResult> DeleteLookupTypeAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/lookup-types/{id}");
        return response.IsSuccessStatusCode
            ? SuccessfulResult.Succeed("Designation deleted successfully.")
            : await response.ToResult();
    }
}