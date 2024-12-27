using Personals.Common.Contracts.CodeSnippets;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;

namespace Personals.CodeSnippets.Abstractions.Services;

public interface ICodeSnippetService
{
    Task<PaginatedResult<CodeSnippetResponse>> GetAllCodeSnippetsAsync(int page, int pageSize, string? search = null);
    
    Task<IResult<CodeSnippetResponse>> GetCodeSnippetByIdAsync(Guid id);
    
    Task<IResult<CodeSnippetResponse>> CreateCodeSnippetAsync(CreateCodeSnippetRequest request);
    
    Task<IResult<CodeSnippetResponse>> UpdateCodeSnippetAsync(Guid id, UpdateCodeSnippetRequest request);
    
    Task<IResult> DeleteCodeSnippetAsync(Guid id);
}