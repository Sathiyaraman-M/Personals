using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Enums;
using Expensive.Common.Wrappers;
using Expensive.Common.Wrappers.Abstractions;
using Expensive.Infrastructure.Abstractions.Repositories;
using Expensive.Infrastructure.Abstractions.Services;
using Expensive.LookupTypes.Abstractions.Repositories;
using Expensive.LookupTypes.Abstractions.Services;
using Expensive.LookupTypes.Entities;
using Expensive.LookupTypes.Extensions;
using Expensive.LookupTypes.Repositories;

namespace Expensive.LookupTypes.Services;

public class LookupTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : ILookupTypeService
{
    private readonly ILookupTypeRepository _lookupTypeRepository =
        unitOfWork.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>();

    public async Task<PaginatedResult<LookupTypeResponse>> GetAllLookupTypesAsync(LookupTypeCategory category, int page,
        int pageSize, string? searchString = null)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be greater than 0", nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
        }

        var designations = (await _lookupTypeRepository.GetAllLookupTypesAsync(category, page, pageSize, searchString))
            .Select(x => x.ToResponse()).ToList();
        var serialNo = ((page - 1) * pageSize) + 1;
        designations.ForEach(x => x.SerialNo = serialNo++);
        var totalLookupTypes = await _lookupTypeRepository.GetLookupTypesCountAsync(category, searchString);
        return PaginatedResult<LookupTypeResponse>.Create(designations, page, pageSize, totalLookupTypes);
    }

    public async Task<IResult<LookupTypeResponse>> GetLookupTypeByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty", nameof(id));
        }

        var lookupType = await _lookupTypeRepository.GetLookupTypeByIdAsync(id);
        return SuccessfulResult<LookupTypeResponse>.Succeed(lookupType.ToResponse());
    }

    public async Task<IResult<LookupTypeResponse>> CreateLookupTypeAsync(CreateLookupTypeRequest request)
    {
        var lookupTypeModel = request.ToModel(currentUserService.UserName, currentUserService.UserId);

        unitOfWork.BeginTransaction();
        var lookupTypeId = await _lookupTypeRepository.CreateLookupTypeAsync(lookupTypeModel);
        unitOfWork.CommitChanges();

        return await GetLookupTypeByIdAsync(lookupTypeId);
    }

    public async Task<IResult<LookupTypeResponse>> UpdateLookupTypeAsync(Guid id, UpdateLookupTypeRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty", nameof(id));
        }

        var lookupTypeModel = request.ToModel(currentUserService.UserName, currentUserService.UserId);

        unitOfWork.BeginTransaction();
        await _lookupTypeRepository.UpdateLookupTypeAsync(id, lookupTypeModel);
        unitOfWork.CommitChanges();

        return await GetLookupTypeByIdAsync(id);
    }

    public async Task<IResult> DeleteLookupTypeAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty", nameof(id));
        }

        unitOfWork.BeginTransaction();
        await _lookupTypeRepository.DeleteLookupTypeAsync(id);
        unitOfWork.CommitChanges();

        return SuccessfulResult.Succeed();
    }
}