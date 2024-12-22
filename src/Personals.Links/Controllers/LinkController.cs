using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personals.Common.Contracts.Links;
using Personals.Links.Abstractions.Services;

namespace Personals.Links.Controllers;

[Authorize]
[ApiController]
[Route("api/links")]
public class LinkController(ILinkService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLinksAsync(int page, int pageSize, string? search = null)
    {
        return Ok(await linkService.GetAllLinksAsync(page, pageSize, search));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLinkByIdAsync(Guid id)
    {
        return Ok(await linkService.GetLinkByIdAsync(id));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateLinkAsync(CreateLinkRequest request)
    {
        return Created("", await linkService.CreateLinkAsync(request));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLinkAsync(Guid id, UpdateLinkRequest request)
    {
        return Ok(await linkService.UpdateLinkAsync(id, request));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLinkAsync(Guid id)
    {
        return Ok(await linkService.DeleteLinkAsync(id));
    }
}