using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Enums;
using Expensive.Common.Wrappers;
using Expensive.Common.Wrappers.Abstractions;

namespace Expensive.UI.Abstractions.Services.Http;

public interface ILookupTypeService
{
    Task<PaginatedResult<LookupTypeResponse>> GetAllAsync(LookupTypeCategory category, int page, int pageSize, string? searchText, CancellationToken cancellationToken = default);
    
    Task<IResult<LookupTypeResponse>> GetLookupTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);    
    
    Task<IResult> CreateLookupTypeAsync(CreateLookupTypeRequest model);
    
    Task<IResult> UpdateLookupTypeAsync(Guid id, UpdateLookupTypeRequest model);
    
    Task<IResult> DeleteLookupTypeAsync(Guid id);
}