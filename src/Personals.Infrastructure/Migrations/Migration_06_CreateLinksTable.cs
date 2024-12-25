using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "Personals.Core.Migrations")]
[Migration(6)]
public class Migration_06_CreateLinksTable : Migration
{
    public override void Up()
    {
        Create.Table("Links")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("Url").AsString().NotNullable()
            .WithColumn("Title").AsString(100).Nullable()
            .WithColumn("Description").AsString(2000).Nullable()
            .WithColumn("Tags").AsString().NotNullable()
            .WithColumn("UserId").AsGuid().NotNullable()
            .WithColumn("CreatedOnDate").AsDateTime2().NotNullable()
            .WithColumn("LastModifiedOnDate").AsDateTime2().Nullable();
        Create.ForeignKey("FK_Links_UserId")
            .FromTable("Links").ForeignColumn("UserId")
            .ToTable("AppUsers").PrimaryColumn("Id");
    }

    [ExcludeFromCodeCoverage]
    public override void Down()
    {
        Delete.Table("Links");
    }
}