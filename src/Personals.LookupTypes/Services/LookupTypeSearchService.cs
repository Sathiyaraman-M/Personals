using Personals.Common.Contracts.LookupTypes;
using Personals.Common.Enums;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.LookupTypes.Abstractions.Repositories;
using Personals.LookupTypes.Abstractions.Services;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Repositories;

namespace Personals.LookupTypes.Services;

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