using Personals.Common.Contracts.LookupTypes;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Models;

namespace Personals.LookupTypes.Extensions;

public static class LookupTypeMappingExtensions
{
    public static LookupType ToLookupType(this CreateLookupTypeModel lookupTypeModel, DateTime createdOnDate)
    {
        return new LookupType
        {
            Id = Guid.NewGuid(),
            Category = lookupTypeModel.Category,
            Code = lookupTypeModel.Code,
            Name = lookupTypeModel.Name,
            UserId = lookupTypeModel.CreatedByUserId,
            CreatedByUserName = lookupTypeModel.CreatedByUserName,
            CreatedByUserId = lookupTypeModel.CreatedByUserId,
            CreatedOnDate = createdOnDate
        };
    }

    public static LookupType ToLookupType(this UpdateLookupTypeModel lookupTypeModel, DateTime lastUpdatedOnDate)
    {
        return new LookupType
        {
            Category = lookupTypeModel.Category,
            Code = lookupTypeModel.Code,
            Name = lookupTypeModel.Name,
            LastModifiedByUserName = lookupTypeModel.LastModifiedByUserName,
            LastModifiedByUserId = lookupTypeModel.LastModifiedByUserId,
            LastModifiedOnDate = lastUpdatedOnDate
        };
    }

    public static LookupTypeResponse ToResponse(this LookupType lookupType)
    {
        return new LookupTypeResponse(
            lookupType.Id,
            lookupType.Category,
            lookupType.Code,
            lookupType.Name,
            lookupType.CreatedByUserName,
            lookupType.CreatedOnDate,
            lookupType.LastModifiedByUserName,
            lookupType.LastModifiedOnDate
        );
    }

    public static CreateLookupTypeModel ToModel(this CreateLookupTypeRequest createLookupTypeRequest,
        string createdByUserName, Guid createdByUserId)
    {
        return new CreateLookupTypeModel
        {
            Category = createLookupTypeRequest.Category,
            Code = createLookupTypeRequest.Code,
            Name = createLookupTypeRequest.Name,
            CreatedByUserName = createdByUserName,
            CreatedByUserId = createdByUserId
        };
    }

    public static UpdateLookupTypeModel ToModel(this UpdateLookupTypeRequest updateLookupTypeRequest,
        string lastModifiedByUserName, Guid lastModifiedByUserId)
    {
        return new UpdateLookupTypeModel
        {
            Category = updateLookupTypeRequest.Category,
            Code = updateLookupTypeRequest.Code,
            Name = updateLookupTypeRequest.Name,
            LastModifiedByUserName = lastModifiedByUserName,
            LastModifiedByUserId = lastModifiedByUserId
        };
    }
}