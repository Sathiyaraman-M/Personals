using Personals.Common.Contracts.CodeSnippets;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;
using Personals.UI.Abstractions.Services.Http;
using Personals.UI.Extensions;
using System.Net.Http.Json;

namespace Personals.UI.Services.Http;

public class CodeSnippetService(HttpClient httpClient) : ICodeSnippetService
{
    public async Task<PaginatedResult<CodeSnippetResponse>> GetAllAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/code-snippets?page={page}&pageSize={pageSize}&search={search}", cancellationToken);
        return await response.ToPaginatedResult<CodeSnippetResponse>(cancellationToken);
    }

    public async Task<IResult<CodeSnippetResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/code-snippets/{id}", cancellationToken);
        return await response.ToResult<CodeSnippetResponse>(cancellationToken);
    }

    public async Task<IResult> CreateAsync(CreateCodeSnippetRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/code-snippets", request);
        return await response.ToResult();
    }

    public async Task<IResult> UpdateAsync(Guid id, UpdateCodeSnippetRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/code-snippets/{id}", request);
        return await response.ToResult();
    }

    public async Task<IResult> DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/code-snippets/{id}");
        return await response.ToResult();
    }
}