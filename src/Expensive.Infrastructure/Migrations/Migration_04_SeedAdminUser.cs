using Expensive.Infrastructure.Abstractions.Utilities;
using FluentMigrator;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Expensive.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "PayFlow.Core.Migrations")]

[Migration(4)]
public class Migration_04_SeedAdminUser(IPasswordHasher passwordHasher) : Migration
{
    public override void Up()
    {
        var loginName = "admin";
        var password = new string(loginName.ToCharArray().Select(c => (char)(c == 'a' ? 'A' : c)).ToArray());
        password = string.Concat(password, '@', string.Concat(Enumerable.Range(1, 3).Select(i => i.ToString(CultureInfo.InvariantCulture))));
        var userId = Guid.NewGuid();
        Insert.IntoTable("AppUsers").Row(new
        {
            Id = userId,
            Code = "admin",
            LoginName = loginName,
            FullName = "Administrator",
            Address1 = "123 Admin Street",
            City = "Admin City",
            PostCode = "12345",
            StateCode = "NY",
            EmailAddress = "admin@example.com",
            PhoneNumber = "1234567890",
            IsActive = true,
            CreatedByUserName = "System",
            CreatedByUserId = Guid.Empty,
            CreatedOnDate = DateTime.UtcNow,
            PasswordHash = passwordHasher.HashPassword(password)
        });
        foreach (var permission in Common.Constants.Permissions.GetAllPermissions())
        {
            Insert.IntoTable("AppUserPermissions").Row(new
            {
                AppUserId = userId,
                Permission = permission
            });
        }
    }

    [ExcludeFromCodeCoverage]
    public override void Down()
    {
        Delete.FromTable("AppUsers").Row(new { LoginName = "admin", CreatedByUserName = "System" });
        Execute.Sql("DELETE FROM [dbo].[AppUserPermissions] WHERE [AppUserId] = (SELECT [Id] FROM [dbo].[AppUsers] WHERE [LoginName] = 'admin' AND [CreatedByUserName] = 'System')");
    }
}