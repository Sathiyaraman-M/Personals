using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Enums;
using Expensive.Infrastructure.Abstractions.Repositories;
using Expensive.LookupTypes.Abstractions.Repositories;
using Expensive.LookupTypes.Abstractions.Services;
using Expensive.LookupTypes.Entities;
using Expensive.LookupTypes.Repositories;

namespace Expensive.LookupTypes.Services;

public class LookupTypeSearchService(IUnitOfWork unitOfWork) : ILookupTypeSearchService
{
    private readonly ILookupTypeRepository _lookupTypeRepository =
        unitOfWork.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>();

    public async Task<List<LookupTypeSearchResponse>> SearchExpenseTypesAsync(string searchString)
    {
        var lookupTypes =
            await _lookupTypeRepository.GetAllLookupTypesAsync(LookupTypeCategory.ExpenseType, 1, 5, searchString);
        return lookupTypes.Select(x => new LookupTypeSearchResponse(x.Id, x.Code, x.Name)).ToList();
    }

    public async Task<List<LookupTypeSearchResponse>> SearchPaymentMethodsAsync(string searchString)
    {
        var lookupTypes =
            await _lookupTypeRepository.GetAllLookupTypesAsync(LookupTypeCategory.PaymentMethod, 1, 5, searchString);
        return lookupTypes.Select(x => new LookupTypeSearchResponse(x.Id, x.Code, x.Name)).ToList();
    }
}