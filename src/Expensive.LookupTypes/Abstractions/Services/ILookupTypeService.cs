using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Enums;
using Expensive.Common.Wrappers;
using Expensive.Common.Wrappers.Abstractions;

namespace Expensive.LookupTypes.Abstractions.Services;

public interface ILookupTypeService
{
    Task<PaginatedResult<LookupTypeResponse>> GetAllLookupTypesAsync(LookupTypeCategory category, int page,
        int pageSize,
        string? searchString = null);

    Task<IResult<LookupTypeResponse>> GetLookupTypeByIdAsync(Guid id);

    Task<IResult<LookupTypeResponse>> CreateLookupTypeAsync(CreateLookupTypeRequest request);

    Task<IResult<LookupTypeResponse>> UpdateLookupTypeAsync(Guid id, UpdateLookupTypeRequest request);

    Task<IResult> DeleteLookupTypeAsync(Guid id);
}