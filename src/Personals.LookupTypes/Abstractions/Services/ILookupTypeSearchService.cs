using Personals.Common.Contracts.LookupTypes;

namespace Personals.LookupTypes.Abstractions.Services;

public interface ILookupTypeSearchService
{
    Task<List<LookupTypeSearchResponse>> SearchExpenseTypesAsync(string searchString);

    Task<List<LookupTypeSearchResponse>> SearchPaymentMethodsAsync(string searchString);
}