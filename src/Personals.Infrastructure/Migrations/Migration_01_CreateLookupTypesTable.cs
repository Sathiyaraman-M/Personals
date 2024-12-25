using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "Personals.Core.Migrations")]

[Migration(1)]
public class Migration_01_CreateLookupTypesTable : Migration
{
    public override void Up()
    {
        Create.Table("LookupTypes")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("Category").AsInt32().NotNullable()
            .WithColumn("Code").AsString(20).Unique().Nullable()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("CreatedByUserName").AsString(100).NotNullable()
            .WithColumn("CreatedByUserId").AsGuid().NotNullable()
            .WithColumn("CreatedOnDate").AsDateTime2().NotNullable()
            .WithColumn("LastModifiedByUserName").AsString(100).Nullable()
            .WithColumn("LastModifiedByUserId").AsGuid().Nullable()
            .WithColumn("LastModifiedOnDate").AsDateTime2().Nullable();
    }

    [ExcludeFromCodeCoverage]
    public override void Down()
    {
        Delete.Table("LookupTypes");
    }
}