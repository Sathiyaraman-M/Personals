using FluentMigrator;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Expensive.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "PayFlow.Core.Migrations")]

[Migration(3)]
public class Migration_03_CreateAppUserPermissionsTable : Migration
{
    public override void Up()
    {
        Create.Table("AppUserPermissions")
            .WithColumn("AppUserId").AsGuid().NotNullable()
            .WithColumn("Permission").AsString().NotNullable();

        Create.PrimaryKey("PK_AppUserPermissions").OnTable("AppUserPermissions").Columns("AppUserId", "Permission");

        Create.ForeignKey("FK_AppUserPermissions_AppUsers_AppUserId")
            .FromTable("AppUserPermissions").ForeignColumn("AppUserId")
            .ToTable("AppUsers").PrimaryColumn("Id").OnDeleteOrUpdate(Rule.Cascade);
    }

    [ExcludeFromCodeCoverage]
    public override void Down()
    {
        Delete.Table("AppUserPermissions");
    }
}