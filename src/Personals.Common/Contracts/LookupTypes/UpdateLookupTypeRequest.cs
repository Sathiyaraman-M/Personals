using Personals.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.LookupTypes;

public class UpdateLookupTypeRequest
{
    public UpdateLookupTypeRequest()
    {
    }

    public UpdateLookupTypeRequest(LookupTypeCategory category, string code, string name)
    {
        Category = category;
        Code = code;
        Name = name;
    }

    [Required(ErrorMessage = "Please select a category")]
    public LookupTypeCategory Category { get; set; }

    [Required(ErrorMessage = "Please enter a code")]
    [MaxLength(20, ErrorMessage = "Code must be less than 20 characters")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Please enter a name")]
    public string Name { get; set; } = null!;
}