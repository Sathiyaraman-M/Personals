using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "PayFlow.Core.Migrations")]
[Migration(5)]
public class Migration_05_CreateLookupTypesTable : Migration
{
    public override void Up()
    {
        Delete.FromTable("LookupTypes").AllRows();
        Alter.Table("LookupTypes").AddColumn("UserId").AsGuid().NotNullable();
        Delete.Index("IX_LookupTypes_Code").OnTable("LookupTypes");
        Create.UniqueConstraint("CodeAndUserId").OnTable("LookupTypes").Columns("Code", "UserId");
    }

    [ExcludeFromCodeCoverage]
    public override void Down()
    {
        Delete.UniqueConstraint("CodeAndUserId").FromTable("LookupTypes");
        Delete.Column("UserId").FromTable("LookupTypes");
        Create.Index("IX_LookupTypes_Code").OnTable("LookupTypes").OnColumn("Code").Unique();
    }
}