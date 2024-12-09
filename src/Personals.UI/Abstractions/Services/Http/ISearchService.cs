using Personals.Common.Contracts.LookupTypes;
using Personals.Common.Wrappers.Abstractions;

namespace Personals.UI.Abstractions.Services.Http;

public interface ISearchService
{
    Task<IResult<List<LookupTypeSearchResponse>>> SearchPaymentMethodsAsync(string searchTerm);
    
    Task<IResult<List<LookupTypeSearchResponse>>> SearchExpenseTypesAsync(string searchTerm);
}