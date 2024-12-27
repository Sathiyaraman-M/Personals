using Personals.CodeSnippets.Abstractions.Repositories;
using Personals.CodeSnippets.Abstractions.Services;
using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Extensions;
using Personals.CodeSnippets.Repositories;
using Personals.Common.Contracts.CodeSnippets;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;
using Personals.Infrastructure.Abstractions.Repositories;

namespace Personals.CodeSnippets.Services;

public class CodeSnippetService(IUnitOfWork unitOfWork) : ICodeSnippetService
{
    private readonly ICodeSnippetRepository _codeSnippetRepository =
        unitOfWork.Repository<CodeSnippet, ICodeSnippetRepository, CodeSnippetRepository>();
    
    public async Task<PaginatedResult<CodeSnippetResponse>> GetAllCodeSnippetsAsync(int page, int pageSize, string? search = null)
    {
        if (page < 1)
        {
            throw new ArgumentException("Invalid page");
        }
        if (pageSize < 1)
        {
            throw new ArgumentException("Invalid page size");
        }
        var codeSnippets = (await _codeSnippetRepository.GetAllCodeSnippetsAsync(page, pageSize, search)).ToList();
        var count = await _codeSnippetRepository.GetCodeSnippetsCountAsync(search);
        var codeSnippetResponses = codeSnippets.Select(codeSnippet => codeSnippet.ToResponse()).ToList();
        var serialNo = (page - 1) * pageSize + 1;
        codeSnippetResponses.ForEach(codeSnippetResponse => codeSnippetResponse.SerialNo = serialNo++);
        return PaginatedResult<CodeSnippetResponse>.Create(codeSnippetResponses, page, pageSize, count);
    }

    public async Task<IResult<CodeSnippetResponse>> GetCodeSnippetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid code snippet id");
        }
        var codeSnippet = await _codeSnippetRepository.GetCodeSnippetByIdAsync(id);
        return SuccessfulResult<CodeSnippetResponse>.Succeed(codeSnippet.ToResponse());
    }

    public async Task<IResult<CodeSnippetResponse>> CreateCodeSnippetAsync(CreateCodeSnippetRequest request)
    {
        var model = request.ToModel();
        
        unitOfWork.BeginTransaction();
        var codeSnippetId = await _codeSnippetRepository.CreateCodeSnippetAsync(model);
        unitOfWork.CommitChanges();
        
        return await GetCodeSnippetByIdAsync(codeSnippetId);
    }

    public async Task<IResult<CodeSnippetResponse>> UpdateCodeSnippetAsync(Guid id, UpdateCodeSnippetRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid code snippet id");
        }
        var model = request.ToModel();
        
        unitOfWork.BeginTransaction();
        await _codeSnippetRepository.UpdateCodeSnippetAsync(id, model);
        unitOfWork.CommitChanges();
        
        return await GetCodeSnippetByIdAsync(id);
    }

    public async Task<IResult> DeleteCodeSnippetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid code snippet id");
        }
        _ = await GetCodeSnippetByIdAsync(id);
        
        unitOfWork.BeginTransaction();
        await _codeSnippetRepository.DeleteCodeSnippetAsync(id);
        unitOfWork.CommitChanges();
        
        return SuccessfulResult.Succeed("Code snippet deleted successfully");
    }
}