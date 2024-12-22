using Personals.Common.Contracts.Links;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;

namespace Personals.UI.Abstractions.Services.Http;

public interface ILinkService
{
    Task<PaginatedResult<LinkResponse>> GetAllAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    
    Task<IResult<LinkResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IResult> CreateAsync(CreateLinkRequest request);
    
    Task<IResult> UpdateAsync(Guid id, UpdateLinkRequest request);
    
    Task<IResult> DeleteAsync(Guid id);
}