using Expensive.Common.Constants;
using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Enums;
using Expensive.Infrastructure.Permissions;
using Expensive.LookupTypes.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expensive.LookupTypes.Controllers;

[ApiController]
[Route("api/lookup-types")]
public class LookupTypeController(ILookupTypeService lookupTypeService) : ControllerBase
{
    [HttpGet("{lookupTypeCategory}")]
    [Permission(Permissions.LookupTypes.View)]
    public async Task<IActionResult> GetSpecifiedLookupTypesAsync(string lookupTypeCategory, int page = 1,
        int pageSize = 10, string? searchText = null)
    {
        var category = GetLookupTypeCategoryFromString(lookupTypeCategory);
        return Ok(await lookupTypeService.GetAllLookupTypesAsync(category, page, pageSize, searchText));
    }

    [HttpGet("{id:guid}")]
    [Permission(Permissions.LookupTypes.View)]
    public async Task<IActionResult> GetLookupTypeAsync(Guid id)
    {
        return Ok(await lookupTypeService.GetLookupTypeByIdAsync(id));
    }

    [HttpPost]
    [Permission(Permissions.LookupTypes.Create)]
    public async Task<IActionResult> CreateLookupTypeAsync(CreateLookupTypeRequest lookupTypeRequest)
    {
        var result = await lookupTypeService.CreateLookupTypeAsync(lookupTypeRequest);
        return Created("/api/lookup-types", result);
    }

    [HttpPut("{id:guid}")]
    [Permission(Permissions.LookupTypes.Update)]
    public async Task<IActionResult> UpdateLookupTypeAsync(Guid id, UpdateLookupTypeRequest lookupTypeRequest)
    {
        return Ok(await lookupTypeService.UpdateLookupTypeAsync(id, lookupTypeRequest));
    }

    [HttpDelete("{id:guid}")]
    [Permission(Permissions.LookupTypes.Delete)]
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