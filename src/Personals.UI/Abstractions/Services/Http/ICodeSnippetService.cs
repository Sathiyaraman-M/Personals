using Personals.Common.Contracts.CodeSnippets;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;

namespace Personals.UI.Abstractions.Services.Http;

public interface ICodeSnippetService
{
    Task<PaginatedResult<CodeSnippetResponse>> GetAllAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    
    Task<IResult<CodeSnippetResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IResult> CreateAsync(CreateCodeSnippetRequest request);
    
    Task<IResult> UpdateAsync(Guid id, UpdateCodeSnippetRequest request);
    
    Task<IResult> DeleteAsync(Guid id);
}