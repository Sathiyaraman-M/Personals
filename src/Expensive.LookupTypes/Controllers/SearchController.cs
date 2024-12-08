using Expensive.LookupTypes.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expensive.LookupTypes.Controllers;

[ApiController]
[Route("api/lookup-types/search")]
public class SearchController(ILookupTypeSearchService lookupTypeSearchService) : ControllerBase
{
    [HttpGet("expense-types")]
    public async Task<IActionResult> SearchExpenseTypesAsync(string searchTerm = "")
    {
        return Ok(await lookupTypeSearchService.SearchExpenseTypesAsync(searchTerm));
    }

    [HttpGet("payment-methods")]
    public async Task<IActionResult> SearchPaymentMethodsAsync(string searchTerm = "")
    {
        return Ok(await lookupTypeSearchService.SearchPaymentMethodsAsync(searchTerm));
    }
}