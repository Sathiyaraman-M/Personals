namespace Personals.Common.Contracts.Users;

public class UpdateUserPermissionsRequest
{
    public List<string> Permissions { get; set; } = [];
}