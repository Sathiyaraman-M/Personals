using Microsoft.AspNetCore.Authorization;
using Personals.Common.Constants;
using Personals.Common.Contracts.LookupTypes;
using Personals.Common.Enums;
using Personals.Infrastructure.Permissions;
using Microsoft.AspNetCore.Mvc;
using Personals.LookupTypes.Abstractions.Services;

namespace Personals.LookupTypes.Controllers;

[Authorize]
[ApiController]
[Route("api/lookup-types")]
public class LookupTypeController(ILookupTypeService lookupTypeService) : ControllerBase
{
    [HttpGet("{lookupTypeCategory}")]
    public async Task<IActionResult> GetSpecifiedLookupTypesAsync(string lookupTypeCategory, int page = 1,
        int pageSize = 10, string? searchText = null)
    {
        var category = GetLookupTypeCategoryFromString(lookupTypeCategory);
        return Ok(await lookupTypeService.GetAllLookupTypesAsync(category, page, pageSize, searchText));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLookupTypeAsync(Guid id)
    {
        return Ok(await lookupTypeService.GetLookupTypeByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> CreateLookupTypeAsync(CreateLookupTypeRequest lookupTypeRequest)
    {
        var result = await lookupTypeService.CreateLookupTypeAsync(lookupTypeRequest);
        return Created("/api/lookup-types", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLookupTypeAsync(Guid id, UpdateLookupTypeRequest lookupTypeRequest)
    {
        return Ok(await lookupTypeService.UpdateLookupTypeAsync(id, lookupTypeRequest));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLookupTypeAsync(Guid id)
    {
        await lookupTypeService.DeleteLookupTypeAsync(id);
        return NoContent();
    }

    private static LookupTypeCategory GetLookupTypeCategoryFromString(string lookupTypeCategory)
    {
        return lookupTypeCategory switch
        {
            "expense-types" => LookupTypeCategory.ExpenseType,
            "payment-methods" => LookupTypeCategory.PaymentMethod,
            _ => throw new ArgumentException("Invalid lookup type category specified!")
        };
    }
}