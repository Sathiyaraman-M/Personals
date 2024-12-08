using Expensive.Common.Contracts.LookupTypes;

namespace Expensive.LookupTypes.Abstractions.Services;

public interface ILookupTypeSearchService
{
    Task<List<LookupTypeSearchResponse>> SearchExpenseTypesAsync(string searchString);

    Task<List<LookupTypeSearchResponse>> SearchPaymentMethodsAsync(string searchString);
}