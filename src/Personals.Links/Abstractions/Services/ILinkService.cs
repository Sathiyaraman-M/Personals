using Personals.Common.Contracts.Links;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;

namespace Personals.Links.Abstractions.Services;

public interface ILinkService
{
    Task<PaginatedResult<LinkResponse>> GetAllLinksAsync(int page, int pageSize, string? search = null);
    
    Task<IResult<LinkResponse>> GetLinkByIdAsync(Guid id);
    
    Task<IResult<LinkResponse>> CreateLinkAsync(CreateLinkRequest request);
    
    Task<IResult<LinkResponse>> UpdateLinkAsync(Guid id, UpdateLinkRequest request);
    
    Task<IResult> DeleteLinkAsync(Guid id);
}