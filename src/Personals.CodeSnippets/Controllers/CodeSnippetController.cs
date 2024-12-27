using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personals.CodeSnippets.Abstractions.Services;
using Personals.Common.Contracts.CodeSnippets;

namespace Personals.CodeSnippets.Controllers;

[Authorize]
[ApiController]
[Route("api/code-snippets")]
public class CodeSnippetController(ICodeSnippetService codeSnippetService): ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllCodeSnippetsAsync(int page, int pageSize, string? search = null)
    {
        return Ok(await codeSnippetService.GetAllCodeSnippetsAsync(page, pageSize, search));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCodeSnippetByIdAsync(Guid id)
    {
        return Ok(await codeSnippetService.GetCodeSnippetByIdAsync(id));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCodeSnippetAsync(CreateCodeSnippetRequest request)
    {
        return Created("", await codeSnippetService.CreateCodeSnippetAsync(request));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCodeSnippetAsync(Guid id, UpdateCodeSnippetRequest request)
    {
        return Ok(await codeSnippetService.UpdateCodeSnippetAsync(id, request));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCodeSnippetAsync(Guid id)
    {
        return Ok(await codeSnippetService.DeleteCodeSnippetAsync(id));
    }
}