using Personals.Infrastructure.Abstractions.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Users.Entities;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1711:Rename type name 'AppUserPermission' so that it does not end in 'Permission'",
    Justification = "This is a domain entity.")]
public record AppUserPermission : Entity
{
    public Guid AppUserId { get; set; }

    public string Permission { get; set; } = null!;
}