using Personals.Common.Enums;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Models;

namespace Personals.LookupTypes.Abstractions.Repositories;

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