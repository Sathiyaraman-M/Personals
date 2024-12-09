using Personals.Common.Contracts.LookupTypes;
using Personals.Common.Enums;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;

namespace Personals.LookupTypes.Abstractions.Services;

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