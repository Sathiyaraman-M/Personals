using Expensive.Common.Enums;
using Expensive.Infrastructure.Abstractions.Repositories;
using Expensive.LookupTypes.Entities;
using Expensive.LookupTypes.Models;

namespace Expensive.LookupTypes.Abstractions.Repositories;

public interface ILookupTypeRepository : IRepository<LookupType>
{
    Task<IEnumerable<LookupType>> GetAllLookupTypesAsync(LookupTypeCategory category, int page, int pageSize,
        string? searchString = null);

    Task<LookupType> GetLookupTypeByIdAsync(Guid id);

    Task<Guid> CreateLookupTypeAsync(CreateLookupTypeModel createLookupTypeModel);

    Task UpdateLookupTypeAsync(Guid id, UpdateLookupTypeModel updateLookupTypeModel);

    Task DeleteLookupTypeAsync(Guid id);

    Task<int> GetLookupTypesCountAsync(LookupTypeCategory category, string? searchString = null);
}