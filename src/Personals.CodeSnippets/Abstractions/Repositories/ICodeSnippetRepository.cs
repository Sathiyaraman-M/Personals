using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Models;
using Personals.Infrastructure.Abstractions.Repositories;

namespace Personals.CodeSnippets.Abstractions.Repositories;

public interface ICodeSnippetRepository : IRepository<CodeSnippet>
{
    Task<IEnumerable<CodeSnippetModel>> GetAllCodeSnippetsAsync(int page, int pageSize, string? search = null);
    
    Task<long> GetCodeSnippetsCountAsync(string? search = null);
    
    Task<CodeSnippetModel> GetCodeSnippetByIdAsync(Guid id);
    
    Task<Guid> CreateCodeSnippetAsync(CreateCodeSnippetModel model);
    
    Task UpdateCodeSnippetAsync(Guid id, UpdateCodeSnippetModel model);
    
    Task DeleteCodeSnippetAsync(Guid id);
}