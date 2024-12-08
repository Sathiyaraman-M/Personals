using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Wrappers.Abstractions;

namespace Expensive.UI.Abstractions.Services.Http;

public interface ISearchService
{
    Task<IResult<List<LookupTypeSearchResponse>>> SearchPaymentMethodsAsync(string searchTerm);
    
    Task<IResult<List<LookupTypeSearchResponse>>> SearchExpenseTypesAsync(string searchTerm);
}